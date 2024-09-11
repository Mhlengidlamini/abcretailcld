using Azure.Data.Tables;
using ABCRetail.Models;
using ABCRetail.AzureTableService.Interfaces;
using Azure;

namespace ABCRetail.Services
{
    public class RoleService : IRoleService
    {
        private readonly TableClient _tableClient;

        public RoleService(TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient("UserRoles");
            _tableClient.CreateIfNotExists();
        }

        public async Task AddUserRoleAsync(string userId, string role)
        {
            var userRole = new UserRoleEntity
            {
                PartitionKey = role,
                RowKey = userId,
                ETag = ETag.All
            };

            await _tableClient.AddEntityAsync(userRole);
        }

        public async Task<bool> UserHasRoleAsync(string userId, string role)
        {
            var roleEntity = await _tableClient.GetEntityAsync<UserRoleEntity>(role, userId);
            return roleEntity != null;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var queryResults = _tableClient.QueryAsync<UserRoleEntity>(role => role.RowKey == userId);
            var roles = new List<string>();

            await foreach (var entity in queryResults)
            {
                roles.Add(entity.PartitionKey);
            }

            return roles;
        }
    }
}
