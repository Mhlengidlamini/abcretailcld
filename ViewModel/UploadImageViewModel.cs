using System.ComponentModel.DataAnnotations;

namespace ABCRetail.ViewModel
{
    public class UploadImageViewModel
    {
        public string ProductId { get; set; }

        [Required]
        [Display(Name = "Product Image")]
        public IFormFile ImageFile { get; set; }
    }
}
