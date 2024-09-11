using ABCRetail.AzureTableService.Interfaces;
using ABCRetail.Models;
using ABCRetail.Repositories.RepositorieInterfaces;
using ABCRetail.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;

public class AccountController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAdminService _adminService;
    private readonly IRoleService _roleService;

    public AccountController(ICustomerService customerService, IHttpContextAccessor httpContextAccessor, IAdminService adminService, IRoleService roleService)
    {
        _customerService = customerService;
        _httpContextAccessor = httpContextAccessor;
        _adminService = adminService;
        _roleService = roleService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var existingProfile = await _customerService.GetCustomerProfileByEmailAsync(model.Email);
                if (existingProfile != null)
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View(model);
                }
                var rowKey = await _customerService.GetNextCustomerRowKeyAsync();
                var customerProfile = new CustomerProfile
                {
                    PartitionKey = "CustomerProfile",
                    RowKey = await _customerService.GetNextCustomerRowKeyAsync(),
                    Name = model.Name,
                    Email = model.Email,   
                    PhoneNumber = model.PhoneNumber,
                    Password = _customerService.HashPassword(model.Password)
                };

                // save customer
                await _customerService.AddCustomerAsync(customerProfile);

                // Assign the "Customer" role to the newly registered user
                await _roleService.AddUserRoleAsync(customerProfile.RowKey, "Customer");

                // Optionally, log in the user or redirect to a success page
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a logging framework)
                ModelState.AddModelError("", $"An error occurred during registration: {ex.Message}");
            }
        }

        return View(model);
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                bool isAuthenticated = false;

                if (model.Role == "Customer")
                {
                    isAuthenticated = await _customerService.ValidateCustomerCredentialsAsync(model.Email, model.Password);
                    if (isAuthenticated)
                    {
                        var customerProfile = await _customerService.GetCustomerProfileByEmailAsync(model.Email);
                        HttpContext.Session.SetString("UserRole", "Customer");
                        HttpContext.Session.SetString("UserId", customerProfile.RowKey);

                        // Sign in user
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, customerProfile.Name),
                        new Claim(ClaimTypes.Email, customerProfile.Email),
                        new Claim(ClaimTypes.Role, "Customer"),
                        new Claim("UserId", customerProfile.RowKey)
                    };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    }
                }
                else if (model.Role == "Admin")
                {
                    isAuthenticated = await _adminService.ValidateAdminCredentialsAsync(model.Email, model.Password);
                    if (isAuthenticated)
                    {
                        var adminProfile = await _adminService.GetAdminProfileByEmailAsync(model.Email);
                        HttpContext.Session.SetString("UserRole", "Admin");
                        HttpContext.Session.SetString("AdminId", adminProfile.RowKey);

                        // Sign in admin
                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Email, adminProfile.Email),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("AdminId", adminProfile.RowKey)
                    };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    }
                }

                if (isAuthenticated)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid credentials or role.");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                ModelState.AddModelError("", $"An error occurred during login: {ex.Message}");
            }
        }

        return View(model);
    }


    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Account");
    }


}
