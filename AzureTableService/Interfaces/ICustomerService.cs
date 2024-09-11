using ABCRetail.Models;
using ABCRetail.ViewModel;

namespace ABCRetail.Repositories.RepositorieInterfaces
{
    public interface ICustomerService
    {
        Task<List<CustomerProfile>> GetAllCustomersAsync();
        Task<CustomerProfile> GetCustomerByIdAsync(string id);
        Task<CustomerProfile> GetCustomerProfileAsync(string userId);
        Task<CustomerProfile> GetCustomerProfileByEmailAsync(string email);
        Task<bool> ValidateCustomerCredentialsAsync(string email, string password);
        Task AddCustomerAsync(CustomerProfile customer);
        Task UpdateCustomerAsync(CustomerProfile customer);
        Task DeleteCustomerAsync(string id);
        Task<string> GetNextCustomerRowKeyAsync();
        public string HashPassword(string password);
    }
}
