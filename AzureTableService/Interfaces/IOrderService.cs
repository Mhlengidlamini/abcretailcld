using ABCRetail.Models;

namespace ABCRetail.Repositories.RepositorieInterfaces
{
    public interface IOrderService
    {
        Task SaveOrderAsync(Order order, List<OrderItem> items);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<string> GetNextOrderRowKeyAsync();
        Task<string> GetNextOrderItemRowKeyAsync(string orderId);
        Task SaveOrderItemAsync(OrderItem orderItem);
        Task UpdateInventoryAsync(IEnumerable<OrderItem> orderItems);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(string customerId);
        Task<Order> GetOrderByIdAsync(string orderId);
        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(string orderId);
    }
}
