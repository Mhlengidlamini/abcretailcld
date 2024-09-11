using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class AdminProfile : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string AdminId => RowKey;
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
