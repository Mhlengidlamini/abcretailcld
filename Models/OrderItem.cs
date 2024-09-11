using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class OrderItem : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public string ProductName { get; set; }
    }
}