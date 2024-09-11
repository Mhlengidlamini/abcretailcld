using ABCRetail.AzureQueueService.Interfaces;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
namespace ABCRetail.AzureQueueService.Service
{
    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(string connectionString)
        {
            string queueName = "queue-order";
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists();
        }

        // Add a message to queue
        public async Task SendMessageAsync(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                await _queueClient.SendMessageAsync(message);
            }
        }

        // Receive a message from the queue
        public async Task<QueueMessage> ReceiveMessageAsync()
        {
            QueueMessage[] retrievedMessage = await _queueClient.ReceiveMessagesAsync(maxMessages: 1);

            return retrievedMessage.Length > 0 ? retrievedMessage[0] : null;
        }

        // Delete the message from the queue
        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            await _queueClient.DeleteMessageAsync(messageId, popReceipt);
        }
    }
}
