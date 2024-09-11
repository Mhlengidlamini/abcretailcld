using ABCRetail.Models;
using ABCRetail.ViewModel;

namespace ABCRetail.AzureTableService.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(string id);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(string rowKey);
        Task<string> GetNextProductRowKeyAsync();
    }
}
