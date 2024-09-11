using ABCRetail.AzureQueueService.Interfaces;
using ABCRetail.AzureTableService.Interfaces;
using ABCRetail.Models;
using ABCRetail.Repositories.RepositorieInterfaces;
using Azure;
using Azure.Data.Tables;
using System.Linq;
using System.Text.Json;

namespace ABCRetail.Repositories.ServiceClasses
{
    public class OrderService : IOrderService
    {
        private readonly TableClient _orderTableClient;
        private readonly TableClient _orderItemsTableClient;
        private readonly IProductService _productService;
        private readonly IQueueStorageService _queueStorageService;

        public OrderService(TableServiceClient tableServiceClient, IProductService productService, IQueueStorageService queueStorageService)
        {
            // Initialize the TableClient for the Orders table
            _orderTableClient = tableServiceClient.GetTableClient("OrderTable");
            _orderTableClient.CreateIfNotExists();

            // Initialize the TableClient for the OrderItems table
            _orderItemsTableClient = tableServiceClient.GetTableClient("OrderItemsTable");
            _orderItemsTableClient.CreateIfNotExists();

            _productService = productService;
            _queueStorageService = queueStorageService;
        }

        public async Task SaveOrderAsync(Order order, List<OrderItem> items)
        {
            // Save Order
            await _orderTableClient.UpsertEntityAsync(order);

            // Save Order Items
            foreach (var item in items)
            {
                item.PartitionKey = order.RowKey; // Use OrderId as PartitionKey for items
                item.RowKey = await GetNextOrderItemRowKeyAsync(order.RowKey); // Unique identifier for each item
                await _orderItemsTableClient.UpsertEntityAsync(item);
            }
            //create a message to send to the queue
            // Create a simplified message object with necessary properties
            var message = new
            {
                OrderId = order.RowKey,      // OrderId
                CustomerId = order.CustomerId,  // CustomerId
                Items = items.Select(item => new
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            // Serialize the message object to JSON
            var serializedMessage = JsonSerializer.Serialize(message);

            // Send the message to the queue
            await _queueStorageService.SendMessageAsync(serializedMessage);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>())
            {
                orders.Add(order);
            }
            return orders;
        }

        public async Task<string> GetNextOrderRowKeyAsync()
        {
            var orders = await GetAllOrdersAsync();
            var lastOrder = orders.OrderByDescending(o => int.Parse(o.RowKey.Substring(2))).FirstOrDefault();

            if (lastOrder == null)
            {
                return "Or0"; // Start with Or0 if no orders exist
            }

            int nextId = int.Parse(lastOrder.RowKey.Substring(2)) + 1;
            return $"Or{nextId}";
        }

        public async Task<string> GetNextOrderItemRowKeyAsync(string orderId)
        {
            var items = await GetOrderItemsByOrderIdAsync(orderId);
            var lastItem = items.OrderByDescending(i => int.Parse(i.RowKey.Substring(2))).FirstOrDefault();

            if (lastItem == null)
            {
                return $"Oi0"; // Start with Oi0 if no items exist for the order
            }

            int nextId = int.Parse(lastItem.RowKey.Substring(2)) + 1;
            return $"Oi{nextId}";
        }

        public async Task SaveOrderItemAsync(OrderItem orderItem)
        {
            orderItem.RowKey = await GetNextOrderItemRowKeyAsync(orderItem.PartitionKey); // Set the RowKey
            await _orderItemsTableClient.UpsertEntityAsync(orderItem);
        }

        public async Task UpdateInventoryAsync(IEnumerable<OrderItem> orderItems)
        {
            foreach (var item in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockLevel -= item.Quantity;
                    await _productService.UpdateProductAsync(product);
                }
            }
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string customerId)
        {
            var orders = new List<Order>();
            await foreach (var order in _orderTableClient.QueryAsync<Order>(filter: $"CustomerId eq '{customerId}'"))
            {
                orders.Add(order);
            }
            return orders;
        }


        public async Task<Order> GetOrderByIdAsync(string orderId)
        {
            try
            {
                return await _orderTableClient.GetEntityAsync<Order>("Orders", orderId);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            var orderItems = new List<OrderItem>();
            await foreach (var item in _orderItemsTableClient.QueryAsync<OrderItem>(filter: $"PartitionKey eq '{orderId}'"))
            {
                orderItems.Add(item);
            }
            return orderItems;
        }
    }
}
