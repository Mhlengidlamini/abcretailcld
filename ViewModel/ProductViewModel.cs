using System.ComponentModel.DataAnnotations;

namespace ABCRetail.ViewModel
{
    public class ProductViewModel
    {
        public string Id { get; set; } // Corresponds to RowKey
        [Required]
        public string ProductName { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
        public double Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock level must be a positive number")]
        public int StockLevel { get; set; }
        public string ImageUrl { get; set; }
    }
}
