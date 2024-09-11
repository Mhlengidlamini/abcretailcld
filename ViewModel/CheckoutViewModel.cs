using ABCRetail.Models;

namespace ABCRetail.ViewModel
{
    public class CheckoutViewModel
    {
        public string OrderId { get; set; }
        public double TotalAmount { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }
}
