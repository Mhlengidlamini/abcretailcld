using ABCRetail.Models;
using ABCRetail.Repositories.RepositorieInterfaces;
using ABCRetail.ViewModel;
using Azure;
using Azure.Data.Tables;
using System.Security.Cryptography;
using System.Text;

namespace ABCRetail.Repositories.ServiceClasses
{
    public class CustomerService : ICustomerService
    {
        private readonly TableClient _tableClient;

        public CustomerService(TableServiceClient tableServiceClient)
        {
            _tableClient = tableServiceClient.GetTableClient("CustomerProfiles");
            _tableClient.CreateIfNotExists();
        }

        public async Task<List<CustomerProfile>> GetAllCustomersAsync()
        {
            var customers = new List<CustomerProfile>();
            await foreach (var customer in _tableClient.QueryAsync<CustomerProfile>())
            {
                customers.Add(customer);
            }
            return customers;
        }

        public async Task<CustomerProfile> GetCustomerByIdAsync(string id)
        {
            var query = _tableClient.QueryAsync<CustomerProfile>(c => c.PartitionKey == "CustomerProfile" && c.RowKey == id);

            await foreach (var customer in query)
            {
                return customer; // If found, return the customer profile
            }

            return null; // If no customer found, return null
        }


        public async Task<CustomerProfile> GetCustomerProfileByEmailAsync(string email)
        {
            var query = _tableClient.QueryAsync<CustomerProfile>(filter: $"PartitionKey eq 'CustomerProfile' and Email eq '{email}'");
            await foreach (var profile in query)
            {
                return profile;
            }
            return null;
        }

        public async Task AddCustomerAsync(CustomerProfile profile)
        {
            profile.PartitionKey = "CustomerProfile";
            profile.RowKey = await GetNextCustomerRowKeyAsync();
            await _tableClient.AddEntityAsync(profile);
        }

        public async Task<bool> ValidateCustomerCredentialsAsync(string email, string password)
        {
            // Retrieve the customer profile using the provided email
            var profile = await GetCustomerProfileByEmailAsync(email);

            // If the profile exists, verify the provided password against the stored hash
            if (profile != null)
            {
                return VerifyPassword(password, profile.Password);
            }

            // Return false if the profile is not found or the password does not match
            return false;
        }

        public async Task UpdateCustomerAsync(CustomerProfile customer)
        {
            try
            {
                await _tableClient.UpdateEntityAsync(customer, ETag.All, TableUpdateMode.Replace);
            }
            catch (RequestFailedException ex)
            {
                // Handle the error as needed
                Console.WriteLine($"Error editing entity: {ex.Message}");
                throw;
            }
        }
        public async Task DeleteCustomerAsync(string id)
        {
            try 
            {
                await _tableClient.DeleteEntityAsync("Customer", id);
            }
            catch (RequestFailedException ex)
            {
                // Handle the error as needed
                Console.WriteLine($"Error deleting entity: {ex.Message}");
                throw;
            }
        }

        public async Task<CustomerProfile> GetCustomerProfileAsync(string userId)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<CustomerProfile>(userId, userId);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine($"Error finding entity: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetNextCustomerRowKeyAsync()
        {
            var customers = await GetAllCustomersAsync();
            var lastCustomer = customers.OrderByDescending(c => int.Parse(c.RowKey.Substring(1))).FirstOrDefault();
            if (lastCustomer == null)
            {
                return "C0";
            }

            int nextId = int.Parse(lastCustomer.RowKey.Substring(1)) + 1;
            return $"C{nextId}";
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Hash the entered password
            var enteredHash = HashPassword(enteredPassword);

            // Compare the entered hash with the stored hash
            return enteredHash == storedHash;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

    }
}
