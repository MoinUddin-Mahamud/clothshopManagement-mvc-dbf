using System;
using System.ComponentModel.DataAnnotations;

namespace MyClothShopManagement.Models.ViewModels
{
    public class ProductCategoryViewModel
    {
        [Display(Name = "Category ID")]
        public int ProductCategoryId { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Category Name must be between 2 and 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Category Description")]
        [DataType(DataType.MultilineText)]
        public string CategoryDescription { get; set; }

        [Display(Name = "Is Active")]
        public bool? IsActive { get; set; }

        [Display(Name = "Product Count")]
        public int? ProductCount { get; set; }
    }

    public class CategoryAjaxResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ProductCategoryViewModel Data { get; set; }
        public int ErrorCode { get; set; }
    }
}
