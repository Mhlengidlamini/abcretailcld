using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get ; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }

        public string Role { get; set; }
    }
}
