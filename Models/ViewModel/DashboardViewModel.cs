using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MyClothShopManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TodaySales { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TodayOrders { get; set; }
        public int LowStockCount { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; }
        public List<LowStockViewModel> LowStockProducts { get; set; }
        public List<MonthlySalesViewModel> MonthlySales { get; set; }
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class LowStockViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class MonthlySalesViewModel
    {
        public string Month { get; set; }
        public decimal Total { get; set; }
    }

    public class OrderReportViewModel
    {
        public int OrderId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; }

        [Display(Name = "Order Date")]
        [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}", ApplyFormatInEditMode = false)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Qty")]
        public int OrderQuantity { get; set; }

        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
    }

    public class OrderReportFilterViewModel
    {
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        public List<OrderReportViewModel> Results { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
