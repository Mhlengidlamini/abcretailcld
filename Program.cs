using ABCRetail.AzureBlobService.Interface;
using ABCRetail.AzureBlobService.Service;
using ABCRetail.AzureFileService.Interfaces;
using ABCRetail.AzureFileService.Service;
using ABCRetail.AzureQueueService.Interfaces;
using ABCRetail.AzureQueueService.Service;
using ABCRetail.AzureTableService.Interfaces;
using ABCRetail.AzureTableService.ServiceClasses;
using ABCRetail.Repositories.RepositorieInterfaces;
using ABCRetail.Repositories.ServiceClasses;
using ABCRetail.Services;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Make session cookie accessible only via HTTP
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

// Load Azure Storage connection string
var azureConnectionString = builder.Configuration.GetSection("AzureStorage:ConnectionString").Value;

if (string.IsNullOrEmpty(azureConnectionString))
{
    throw new InvalidOperationException("Azure Storage connection string is not configured.");
}

// Configure TableServiceClient
builder.Services.AddSingleton<TableServiceClient>(provider =>
{
    return new TableServiceClient(azureConnectionString);
});

// Configure BlobStorageService
builder.Services.AddSingleton<IBlobStorageService>(sp =>
{
    return new BlobStorageService(azureConnectionString);
});

// Configure ContractsFileService
builder.Services.AddSingleton<IContractsFileService>(sp =>
{
    return new ContractsFileService(azureConnectionString);
});

// Configure LogsFileService
builder.Services.AddSingleton<ILogsFileService>(sp =>
{
    return new LogsFileService(azureConnectionString);
});

// Register the QueueStorageService
builder.Services.AddSingleton<IQueueStorageService>(sp =>
{
    return new QueueStorageService(azureConnectionString);
});


// Register the repository and service for the Azure Tables
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddSingleton<ICustomerService, CustomerService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IRoleService, RoleService>();

// Register the background service for processing queues as scoped
builder.Services.AddSingleton<OrderProcessingService>();
builder.Services.AddHostedService<OrderProcessingService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
