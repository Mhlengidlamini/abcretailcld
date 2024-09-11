using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.ViewModel;
using ABCRetail.AzureBlobService.Interface;
using Microsoft.AspNetCore.Authorization;
using ABCRetail.AzureTableService.Interfaces;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<ProductController> _logger;

    public ProductController(IProductService productService, IBlobStorageService blobStorageService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    // GET: /Product
    // used by both the customer and the admin
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();

            // Convert Product to ProductViewModel
            var viewModel = products.Select(p => new ProductViewModel
            {
                Id = p.RowKey,
                ProductName = p.ProductName,
                Description = p.Description,
                Price = p.Price,
                StockLevel = p.StockLevel,
                ImageUrl = p.ImageUrl
            }).ToList();

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving products.");
            // Handle the error appropriately
            return View("Error"); // Return an error view or handle as needed
        }
    }


    // GET: /Product/Details
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while retrieving product details: {ex.Message}");
            return View();
        }
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult UploadImage()
    {
        return View(new UploadImageViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(IFormFile imageFile)
    {
        if (imageFile != null && imageFile.Length > 0)
        {
            // Upload image to blob storage
            string imageUrl = await _blobStorageService.UploadFileAsync(imageFile.FileName, imageFile.OpenReadStream());

            // Save image URL to TempData or session, to use in the next step
            TempData["ImageUrl"] = imageUrl;

            return RedirectToAction(nameof(Create));
        }

        ModelState.AddModelError("", "Please upload a valid image.");
        return View();
    }

    // GET: /Product/Create
    // this will be used by the admin to add products
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        var imageUrl = TempData["ImageUrl"] as string;
        if (string.IsNullOrEmpty(imageUrl))
        {
            return RedirectToAction(nameof(UploadImage));
        }

        var model = new ProductViewModel
        {
            ImageUrl = imageUrl // Pass the image URL to the view
        };
        return View(model);
    }

    // POST: /Product/Create
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(ProductViewModel model)
    {

        if (string.IsNullOrEmpty(model.Id))
        {
            _logger.LogInformation("Generating a new RowKey (Id).");
            model.Id = await _productService.GetNextProductRowKeyAsync();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Validation Error: The Rowkey field is required.");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("Validation Error: " + error.ErrorMessage);
            }
            return View(model);
        }

        try
        {


            var product = new Product
            {
                RowKey = model.Id,
                PartitionKey = "Product",
                ProductName = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                StockLevel = model.StockLevel,
                ImageUrl = model.ImageUrl
            };

            await _productService.AddProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the product.");
            ModelState.AddModelError("", "An error occurred while creating the product.");
            return View(model);
        }
    }


    // GET: /Product/Edit
    // used by the admin
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound();

        var model = new ProductViewModel
        {
            Id = product.RowKey,
            ProductName = product.ProductName,
            Description = product.Description,
            Price = product.Price,
            StockLevel = product.StockLevel,
            ImageUrl = product.ImageUrl
        };

        return View(model);
    }


    // POST: /Product/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(ProductViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(model.Id);

                if (product == null)
                {
                    ModelState.AddModelError("", "Product not found.");
                    return View(model);
                }

                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.Price = model.Price;
                product.StockLevel = model.StockLevel;

              /*  if (model.ImageFile != null)
                {
                    var fileName = $"{model.Rowkey}_{model.ImageFile.FileName}";
                    product.ImageUrl = await _blobStorageService.UploadFileAsync(fileName, model.ImageFile.OpenReadStream());
                }*/

                await _productService.UpdateProductAsync(product);

                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the product.");
                ModelState.AddModelError("", "An error occurred while updating the product.");
            }
        }
        return View(model);
    }

    // GET: /Product/Delete
    // used by admin
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            // Retrieve the product by id
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                // If the product doesn't exist, redirect to Not Found or a list page
                return RedirectToAction("NotFound", "Home");
            }

            // Return the delete confirmation view, passing the product
            return View(product);
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, "Error retrieving product with id {id} for deletion.", id);

            // Redirect to a generic error page or handle accordingly
            return RedirectToAction("Error", "Home");
        }
    }

}