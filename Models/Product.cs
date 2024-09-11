using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string ProductName { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int StockLevel { get; set; }
        public string ImageUrl { get; set; }
    }

}
