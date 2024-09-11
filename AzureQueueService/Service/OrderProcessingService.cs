using ABCRetail.AzureQueueService.Interfaces;
using ABCRetail.Models;
using ABCRetail.Repositories.RepositorieInterfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace ABCRetail.AzureQueueService.Service

{
    public class OrderProcessingService : BackgroundService
    {
        private readonly IQueueStorageService _queueStorageService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OrderProcessingService> _logger;

        public OrderProcessingService(IQueueStorageService queueStorageService, IServiceScopeFactory serviceScopeFactory, ILogger<OrderProcessingService> logger)
        {
            _queueStorageService = queueStorageService;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           // throw new NotImplementedException();
           while(!stoppingToken.IsCancellationRequested)
            {
                //deserialize the message into OrderData
                var message = await _queueStorageService.ReceiveMessageAsync();
                if (message != null)
                {
                    try
                    {
                        var orderData = JsonSerializer.Deserialize<Order>(message.MessageText);
                        if (orderData != null)
                        {
                            // Create a scope to resolve IOrderService
                            using (var scope = _serviceScopeFactory.CreateScope())
                            {
                                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

                                // Get the associated order items
                                var orderItems = await orderService.GetOrderItemsByOrderIdAsync(orderData.RowKey);

                                // Process the order by updating the inventory
                                await orderService.UpdateInventoryAsync(orderItems);

                                // Delete the message from the queue after processing
                                await _queueStorageService.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        // log any errors during processing
                        _logger.LogError(ex, "Error processing message: {MessageId}", message.MessageId);
                    }
                }

                // waiting period before checking the queue again
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);  
            }
        }
    }
}
