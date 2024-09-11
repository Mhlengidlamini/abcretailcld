using Azure.Storage.Queues.Models;

namespace ABCRetail.AzureQueueService.Interfaces
{
    public interface IQueueStorageService
    {
        Task SendMessageAsync(string message);
        Task<QueueMessage> ReceiveMessageAsync();
        Task DeleteMessageAsync(string messageId, string popRecipt);
    }
}
