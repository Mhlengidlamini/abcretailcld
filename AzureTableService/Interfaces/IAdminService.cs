using ABCRetail.Models;

namespace ABCRetail.AzureTableService.Interfaces
{
    public interface IAdminService
    {
        Task<bool> ValidateAdminCredentialsAsync(string email, string password);
        Task<AdminProfile> GetAdminProfileByEmailAsync(string email);
        Task CreateAdminAsync(AdminProfile adminProfile);
    }
}
