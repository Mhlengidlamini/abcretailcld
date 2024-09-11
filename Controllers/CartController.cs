using ABCRetail.Models;
using ABCRetail.Extensions;
using ABCRetail.Repositories.RepositorieInterfaces;
using Microsoft.AspNetCore.Mvc;
using ABCRetail.ViewModel;
using ABCRetail.AzureTableService.Interfaces;

namespace ABCRetail.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;

        public CartController(IProductService productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            var cart = GetCart(); // Retrieve cart from session
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null || product.StockLevel <= 0)
                {
                    // if product is out of stock or not available
                    return RedirectToAction("Index", "Product");
                }

                var cart = GetCart();
                var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == id);

                if (cartItem == null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = product.RowKey,
                        ProductName = product.ProductName,
                        Price = product.Price,
                        Quantity = 1
                    });
                }
                else
                {
                    cartItem.Quantity++;
                }

                SaveCart(cart);
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                // Log exception and optionally display an error message to the user
                ModelState.AddModelError("", "An error occurred while adding the item to the cart.");
                return RedirectToAction("Index", "Product");
            }
        }

        [HttpPost]
        public IActionResult UpdateQuantity(string productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    // invalid quantity if it is less than zero
                    return RedirectToAction("Index");
                }

                var cart = GetCart();

                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    cartItem.Quantity = quantity;
                }

                SaveCart(cart); // Save updated cart to session
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log exception and optionally display an error message to the user
                ModelState.AddModelError("", "An error occurred while updating the quantity.");
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult RemoveItem(string productId)
        {
            try
            {
                var cart = GetCart();

                var cartItem = cart.Items.FirstOrDefault(item => item.ProductId == productId);
                if (cartItem != null)
                {
                    cart.Items.Remove(cartItem);
                }

                SaveCart(cart); // Save updated cart to session
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log exception and optionally display an error message to the user
                ModelState.AddModelError("", "An error occurred while removing the item from the cart.");
                return RedirectToAction("Index");
            }
        }

        // Helper methods to get and save the cart in session
        private Cart GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<Cart>("Cart") ?? new Cart();
            return cart;
        }

        private void SaveCart(Cart cart)
        {
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }
    }
}
