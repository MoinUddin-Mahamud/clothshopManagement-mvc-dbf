using System;
using System.ComponentModel.DataAnnotations;

namespace MyClothShopManagement.Models.ViewModels
{
    public class SalesDetailsViewModel
    {
        public int? OrderDetailsId { get; set; }
        public int? OrderId { get; set; }
        public int? OrderQuantity { get; set; }
        public string OrderUnit { get; set; }
        public int? ProductCategoryId { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int? ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 999999, ErrorMessage = "Quantity must be between 1 and 999,999")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Unit Price must be valid")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }
}
