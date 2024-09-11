using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.ViewModel;
using ABCRetail.Repositories.ServiceClasses;
using ABCRetail.Repositories.RepositorieInterfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ABCRetail.AzureTableService.Interfaces;

public class OrderController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;

    public OrderController(IOrderService orderService, ICustomerService customerService)
    {
        _orderService = orderService;
        _customerService = customerService;
    }

    // GET: /Order/OrderHistory
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> OrderHistory()
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetOrdersByCustomerIdAsync(userId);
            var viewModel = orders.Select(o => new OrderViewModel
            {
                OrderId = o.RowKey, // Ensure correct property is used
                TotalAmount = o.TotalAmount,
                OrderDate = o.Timestamp ?? DateTimeOffset.MinValue,
                Status = o.OrderStatus
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while retrieving order history: {ex.Message}");
            return View(new List<OrderViewModel>());
        }
    }

    // GET: /Order/AllOrders
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllOrders()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var viewModel = orders.Select(o => new OrderViewModel
            {
                OrderId = o.RowKey, // Ensure correct property is used
                TotalAmount = o.TotalAmount,
                OrderDate = o.Timestamp ?? DateTimeOffset.MinValue,
                Status = o.OrderStatus
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while retrieving all orders: {ex.Message}");
            return View(new List<OrderViewModel>());
        }
    }

    // GET: /Order/OrderDetails/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> OrderDetails(string id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var orderItems = await _orderService.GetOrderItemsByOrderIdAsync(id);

            var viewModel = new OrderViewModel
            {
                OrderId = order.RowKey, // Ensure correct property is used
                TotalAmount = order.TotalAmount,
                OrderDate = order.Timestamp ?? DateTimeOffset.MinValue,
                Status = order.OrderStatus,
                Items = orderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while retrieving order details: {ex.Message}");
            return View();
        }
    }
}
