using ABCRetail.AzureTableService.Interfaces;
using ABCRetail.AzureTableService.ServiceClasses;
using ABCRetail.Extensions;
using ABCRetail.Models;
using ABCRetail.Repositories.RepositorieInterfaces;
using ABCRetail.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICustomerService customerService, IOrderService orderService, ILogger<CheckoutController> logger)
        {
            _customerService = customerService;
            _orderService = orderService;
            _logger = logger;
        }

        // GET: Checkout/Index
        public async Task<IActionResult> Index()
        {
            // Assuming the cart is stored in the session
            var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart");

            if (cart == null)
            {
                // Redirect to some error page or show a message if cart is empty
                return RedirectToAction("EmptyCart", "Cart");
            }

            var checkoutViewModel = new CheckoutViewModel
            {
                Items = cart.Items.Select(item => new OrderItemViewModel
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList(),
                TotalAmount = cart.Items.Sum(item => item.Quantity * item.Price)
            };

            return View(checkoutViewModel);
        }

        // GET: Checkout/Payment
        [HttpGet]
        public IActionResult Payment()
        {
            return View();
        }

        // POST: Checkout/Payment
        [HttpPost]
        public async Task<IActionResult> Payment(PaymentViewModel paymentModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Log payment details
                    _logger.LogInformation("Processing payment for Card Holder: {CardHolderName}", paymentModel.CardHolderName);

                    // Simulate payment processing (no actual payment gateway integration)

                    // Retrieve cart and create order items
                    var cart = GetCart();
                    var orderItems = cart.Items.Select(item => new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price, // Assuming you have Price in cart items
                        ProductName = item.ProductName
                    }).ToList();

                    // Create an Order object
                    var order = new Order
                    {
                        RowKey = await _orderService.GetNextOrderRowKeyAsync(), // Use GetNextOrderRowKeyAsync to get the next RowKey
                        PartitionKey = "Orders",
                        CustomerId = HttpContext.Session.GetString("UserId"),
                        TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity),
                        OrderStatus = "Complete"
                    };

                    // Save the order and order items to Azure Table Storage
                    await _orderService.SaveOrderAsync(order, orderItems);

                    // Clear the cart after order is processed
                    ClearCart();

                    // Store success message in TempData
                    TempData["SuccessMessage"] = "Your payment was processed successfully.";

                    // Redirect to order completion page
                    return RedirectToAction("OrderCompletion");
                }
                catch (Exception ex)
                {
                    // Log the error
                    _logger.LogError(ex, "An error occurred while processing the payment.");

                    // Store error message in TempData
                    TempData["ErrorMessage"] = "An error occurred while processing your payment. Please try again.";

                    // Redirect back to the payment page
                    return View("Payment", paymentModel);
                }
            }

            // If model state is not valid, return the payment form with validation errors
            return View("Payment", paymentModel);
        }


        // GET: Checkout/OrderCompletion
        public IActionResult OrderCompletion()
        {
            return View();
        }

        // Helper methods for managing cart
        private Cart GetCart()
        {
            // Retrieve the cart from session or create a new cart if none exists
            return HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
        }

        private void ClearCart()
        {
            // Clear the cart from the session after checkout
            HttpContext.Session.Remove("Cart");
        }
    }
}
