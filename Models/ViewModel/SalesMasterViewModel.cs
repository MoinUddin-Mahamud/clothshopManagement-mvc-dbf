using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyClothShopManagement.Models.ViewModels
{
    public class SalesMasterViewModel
    {
        [Display(Name = "Order ID")]
        public int SaleId { get; set; }

        [Display(Name = "Order ID")]
        public int OrderId { get; set; }

        [Display(Name = "Customer ID")]
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Customer Name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Customer Name must be between 2 and 200 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "Phone number must be 10-15 digits")]
        [Display(Name = "Contact Number")]
        public string Phone { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        [Display(Name = "Contact Address")]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Order Date is required")]
        [Display(Name = "Order Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Order Items")]
        public List<SalesDetailsViewModel> SalesDetailsList { get; set; }
    }
}
