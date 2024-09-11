using Azure.Data.Tables;
using Azure;

namespace ABCRetail.Models
{
    public class UserRoleEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // Role Name (e.g., "Admin", "Customer")
        public string RowKey { get; set; } // User ID
        public ETag ETag { get; set; } 
        public DateTimeOffset? Timestamp { get; set; }
    }
}
