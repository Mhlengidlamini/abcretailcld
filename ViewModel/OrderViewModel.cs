namespace ABCRetail.ViewModel
{
    public class OrderViewModel
    {
        public string OrderId { get; set; }
        public DateTimeOffset? OrderDate { get; set; }
        public double TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemViewModel> Items { get; set; }
    }
}
