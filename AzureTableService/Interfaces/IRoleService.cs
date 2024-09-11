namespace ABCRetail.AzureTableService.Interfaces
{
    public interface IRoleService
    {
        Task AddUserRoleAsync(string userId, string role);
        Task<bool> UserHasRoleAsync(string userId, string role);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    }
}
