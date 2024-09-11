using ABCRetail.AzureBlobService.Interface;
using ABCRetail.AzureTableService.Interfaces;
using ABCRetail.Models;
using ABCRetail.ViewModel;
using Azure;
using Azure.Data.Tables;

namespace ABCRetail.AzureTableService.ServiceClasses
{
    public class ProductService : IProductService
    {
        private readonly TableClient _tableClient;
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(TableServiceClient tableServiceClient, IBlobStorageService blobStorageService, ILogger<ProductService> logger)
        {
            _tableClient = tableServiceClient.GetTableClient("ProductTable");
            _tableClient.CreateIfNotExists();
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = new List<Product>();
            await foreach (var product in _tableClient.QueryAsync<Product>(p => p.PartitionKey == "Product"))
            {
                products.Add(product);
            }
            return products;
        }


        public async Task<Product> GetProductByIdAsync(string id)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<Product>("Product", id);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task AddProductAsync(Product product)
        {
            _logger.LogInformation("Adding product to Azure Table Storage with RowKey: {RowKey}", product.RowKey);

            if (product.ImageUrl == null)
            {
                _logger.LogError("ImageUrl is null. Product creation will fail.");
            }

            await _tableClient.AddEntityAsync(product);
            _logger.LogInformation("Product successfully saved to Azure Table Storage.");
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            // Update the product in Table Storage
            await _tableClient.UpdateEntityAsync(product, ETag.All, TableUpdateMode.Replace);
        }

        public async Task DeleteProductAsync(string id)
        {
            var product = await GetProductByIdAsync(id);
            if (product != null)
            {
                await _tableClient.DeleteEntityAsync("Product", id);
            }
        }

        public async Task<string> GetNextProductRowKeyAsync()
        {
            var products = new List<Product>();

            // Query the table to retrieve all products
            await foreach (var product in _tableClient.QueryAsync<Product>(p => p.PartitionKey == "Product"))
            {
                products.Add(product);
            }

            // Find the last product by sorting RowKeys in descending order
            var lastProduct = products.OrderByDescending(p => int.Parse(p.RowKey.Substring(1))).FirstOrDefault();

            // If no products exist, start with "P0"
            if (lastProduct == null)
            {
                return "P0";
            }

            // Increment the numeric part of the last RowKey and return the new RowKey
            int nextId = int.Parse(lastProduct.RowKey.Substring(1)) + 1;
            return $"P{nextId}";
        }

    }
}
