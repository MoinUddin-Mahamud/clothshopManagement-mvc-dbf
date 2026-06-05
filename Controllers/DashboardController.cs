using MyClothShopManagement.Filters;
using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    [SessionAuthorize]
    public class DashboardController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();

        public ActionResult Index()
        {
            var today = DateTime.Today;
            const int LowStockThreshold = 10;

            var vm = new DashboardViewModel
            {
                TotalProducts   = db.Products.Count(p => p.IsActive ?? false),
                TotalCategories = db.ProductCategories.Count(),
                TotalCustomers  = db.Customers.Count(),
                TotalOrders     = db.Orders.Count(),
                TotalRevenue    = db.Orders.Any() ? db.Orders.Sum(o => o.TotalAmount) : 0,
                TodayOrders     = db.Orders.Count(o => System.Data.Entity.DbFunctions.TruncateTime(o.OrderDate) == today),
                TodaySales      = db.Orders
                                    .Where(o => System.Data.Entity.DbFunctions.TruncateTime(o.OrderDate) == today)
                                    .Any()
                                    ? db.Orders
                                        .Where(o => System.Data.Entity.DbFunctions.TruncateTime(o.OrderDate) == today)
                                        .Sum(o => o.TotalAmount)
                                    : 0,
                LowStockCount   = db.Products.Count(p => (p.IsActive ?? false) && p.AvailableQuantity <= LowStockThreshold),

                RecentOrders = db.Orders
                    .Include("Customer")
                    .OrderByDescending(o => o.OrderDate)
                    .Take(8)
                    .Select(o => new RecentOrderViewModel
                    {
                        OrderId      = o.OrderId,
                        CustomerName = o.Customer.CustomerName,
                        OrderDate    = (DateTime)o.OrderDate,
                        TotalAmount  = o.TotalAmount
                    }).ToList(),

                LowStockProducts = db.Products
                    .Include("ProductCategory")
                    .Where(p => (p.IsActive ?? false) && p.AvailableQuantity <= LowStockThreshold)
                    .OrderBy(p => p.AvailableQuantity)
                    .Take(8)
                    .Select(p => new LowStockViewModel
                    {
                        ProductId         = p.ProductId,
                        ProductName       = p.ProductName,
                        CategoryName      = p.ProductCategory.CategoryName,
                        AvailableQuantity = p.AvailableQuantity
                    }).ToList(),

                MonthlySales = BuildMonthlySales()
            };

            return View(vm);
        }

        private List<MonthlySalesViewModel> BuildMonthlySales()
        {
            var sixMonthsAgo = DateTime.Today.AddMonths(-5);
            var start = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var raw = db.Orders
                .Where(o => o.OrderDate >= start)
                .GroupBy(o => new
                {
                    Year  = o.OrderDate.Value.Year,
                    Month = o.OrderDate.Value.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .ToList();

            var result = new List<MonthlySalesViewModel>();
            for (int i = 5; i >= 0; i--)
            {
                var d = DateTime.Today.AddMonths(-i);
                var match = raw.FirstOrDefault(r => r.Year == d.Year && r.Month == d.Month);
                result.Add(new MonthlySalesViewModel
                {
                    Month = d.ToString("MMM yyyy"),
                    Total = match?.Total ?? 0
                });
            }
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
