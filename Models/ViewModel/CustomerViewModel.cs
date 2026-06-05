using System;
using System.ComponentModel.DataAnnotations;

namespace MyClothShopManagement.Models.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must be 10-15 digits")]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Contact Address")]
        [DataType(DataType.MultilineText)]
        public string ContactAddress { get; set; }

        [Display(Name = "Member Since")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = false)]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Total Orders")]
        public int OrderCount { get; set; }

        [Display(Name = "Total Spent")]
        public decimal TotalSpent { get; set; }
    }
}
