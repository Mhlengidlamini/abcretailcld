using Azure.Data.Tables;
using Azure;
using ABCRetail.Models;
using System.Text;
using System.Security.Cryptography;
using ABCRetail.AzureTableService.Interfaces;

namespace ABCRetail.Services
{
    public class AdminService : IAdminService
    {
        private readonly TableClient _adminTableClient;

        public AdminService(TableServiceClient tableServiceClient)
        {
            _adminTableClient = tableServiceClient.GetTableClient("AdminProfile");
            _adminTableClient.CreateIfNotExists();
        }

        public async Task<IEnumerable<AdminProfile>> GetAllAdminsAsync()
        {
            // Define the partition key 
            string partitionKey = "Admin";

            // Query the table for all entities with the specified partition key
            var admins = new List<AdminProfile>();
            await foreach (var admin in _adminTableClient.QueryAsync<AdminProfile>(a => a.PartitionKey == partitionKey))
            {
                admins.Add(admin);
            }

            return admins;
        }

        public async Task<bool> ValidateAdminCredentialsAsync(string email, string password)
        {
            var adminProfile = await GetAdminProfileByEmailAsync(email);
            return adminProfile != null && VerifyPassword(password, adminProfile.Password);
        }

        public async Task<AdminProfile> GetAdminProfileByEmailAsync(string email)
        {
            await foreach (var adminProfile in _adminTableClient.QueryAsync<AdminProfile>(a => a.Email == email))
            {
                return adminProfile; // Return the first matching admin profile
            }

            return null; // Return null if no matching profile is found
        }


        public async Task CreateAdminAsync(AdminProfile adminProfile)
        {
            adminProfile.PartitionKey = "Admin";
            adminProfile.RowKey = await GetNextAdminRowKeyAsync(); 
            adminProfile.Password = HashPassword(adminProfile.Password); // Hash the password

            await _adminTableClient.AddEntityAsync(adminProfile);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            var hashOfInput = HashPassword(inputPassword);
            return string.Equals(hashOfInput, storedHash);
        }

        public async Task<string> GetNextAdminRowKeyAsync()
        {
            // Fetch all admin profiles
            var admins = await GetAllAdminsAsync();

            // Get the last admin based on the numeric part of the RowKey
            var lastAdmin = admins.OrderByDescending(a => int.Parse(a.RowKey.Substring(1))).FirstOrDefault();

            // If there is no admin, start from A0
            if (lastAdmin == null)
            {
                return "A0";
            }

            // Extract the numeric part, increment it, and return the new RowKey
            int nextId = int.Parse(lastAdmin.RowKey.Substring(1)) + 1;
            return $"A{nextId}";
        }

    }
}
