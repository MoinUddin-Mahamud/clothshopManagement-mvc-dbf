using System.ComponentModel.DataAnnotations;
using System.Web;

namespace MyClothShopManagement.Models.ViewModels
{
    public class MyClothProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Product Name must be between 3 and 200 characters")]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [StringLength(50, ErrorMessage = "Size cannot exceed 50 characters")]
        [Display(Name = "Size")]
        public string Size { get; set; }

        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        [Display(Name = "Color")]
        public string Color { get; set; }

        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit Price must be between 0.01 and 999,999.99")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Available Quantity is required")]
        [Range(0, 999999, ErrorMessage = "Available Quantity must be between 0 and 999,999")]
        [Display(Name = "Available Quantity")]
        public int AvailableQuantity { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [StringLength(500, ErrorMessage = "Product Image path cannot exceed 500 characters")]
        [Display(Name = "Product Image")]
        public string ProductImage { get; set; }

        [Display(Name = "Upload Image")]
        public HttpPostedFileBase ImageFile { get; set; }

        [Required(ErrorMessage = "Product Category is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category")]
        [Display(Name = "Product Category")]
        public int ProductCategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }
    }
}
