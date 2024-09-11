using Azure;
using Azure.Data.Tables;
using Microsoft.VisualBasic;

namespace ABCRetail.Models
{
    public class Order : ITableEntity
    {
        public string PartitionKey { get; set; } ="Orders"; 
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerId { get; set; }
        public double TotalAmount { get; set; }
        public string OrderStatus { get; set; } = "Complete";
    }
}
