using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.ViewModel;
using ABCRetail.Repositories.ServiceClasses;
using ABCRetail.Repositories.RepositorieInterfaces;
using Azure;
using Microsoft.AspNetCore.Authorization;

public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // GET: /Customer/MyProfile
    [Authorize(Roles = "Customer")]
    [Route("Customer/MyProfile")]
    public async Task<IActionResult> MyProfile()
    {
        var userId = User.FindFirst("UserId")?.Value;
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var customer = await _customerService.GetCustomerByIdAsync(userId);
        if (customer == null)
        {
            return NotFound();
        }

        var viewModel = new CustomerViewModel
        {
            Id = customer.RowKey,
            Name = customer.Name,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber
        };

        return View(viewModel);
    }


    // GET: /Customer/EditProfile
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> EditProfile()
    {
        try
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if no userId
            }

            var customer = await _customerService.GetCustomerByIdAsync(userId);
            if (customer == null)
            {
                return NotFound();
            }

            var viewModel = new CustomerViewModel
            {
                Id = customer.RowKey,
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while retrieving the profile for editing: {ex.Message}");
            return View();
        }
    }

    // POST: /Customer/EditProfile
    [HttpPost]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(CustomerViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Account"); // Redirect to login if no userId
                }

                var customer = new CustomerProfile
                {
                    PartitionKey = "CustomerProfile",
                    RowKey = userId,
                    Name = model.Name,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                await _customerService.UpdateCustomerAsync(customer);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while updating the profile: {ex.Message}");
            }
        }

        return View(model);
    }

   // GET: /Customer/Delete/5
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var viewModel = new CustomerViewModel
            {
                Id = customer.RowKey, // Assuming RowKey is an integer
                Name = customer.Name,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while retrieving customer details: {ex.Message}");
            return View();
        }
    }

    // POST: /Customer/Delete/5

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Customer")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            await _customerService.DeleteCustomerAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while deleting the customer: {ex.Message}");
            return View();
        }
    } 
}

