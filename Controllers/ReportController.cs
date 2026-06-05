using MyClothShopManagement.Filters;
using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    [SessionAuthorize]
    public class ReportController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();

        public ActionResult Orders(DateTime? fromDate, DateTime? toDate, string customerName)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to   = toDate   ?? DateTime.Today;

            string sql = @"
                SELECT
                    od.OrderDetailsId,
                    o.OrderId,
                    c.CustomerName,
                    o.OrderDate,
                    p.ProductName,
                    od.OrderQuantity,
                    od.UnitPrice,
                    od.Amount
                FROM Orders o
                JOIN Customer     c  ON o.CustomerId  = c.CustomerId
                JOIN OrderDetails od ON o.OrderId      = od.OrderId
                JOIN Product      p  ON od.ProductId   = p.ProductId
                WHERE o.OrderDate >= @from AND o.OrderDate < @to";

            var fromParam = new System.Data.SqlClient.SqlParameter("@from", from);
            var toParam   = new System.Data.SqlClient.SqlParameter("@to", to.AddDays(1));

            var raw = db.Database.SqlQuery<OrderReportRow>(sql, fromParam, toParam).ToList();

            if (!string.IsNullOrWhiteSpace(customerName))
                raw = raw.Where(r => r.CustomerName.IndexOf(customerName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            var results = raw.Select(r => new OrderReportViewModel
            {
                OrderId       = r.OrderId,
                CustomerName  = r.CustomerName,
                OrderDate     = r.OrderDate ?? DateTime.MinValue,
                ProductName   = r.ProductName,
                OrderQuantity = r.OrderQuantity,
                UnitPrice     = r.UnitPrice,
                Amount        = r.Amount
            }).ToList();

            var vm = new OrderReportFilterViewModel
            {
                FromDate     = from,
                ToDate       = to,
                CustomerName = customerName,
                Results      = results,
                GrandTotal   = results.Any() ? results.Sum(r => r.Amount) : 0
            };

            return View(vm);
        }

        public ActionResult ExportCsv(DateTime? fromDate, DateTime? toDate, string customerName)
        {
            var from = fromDate ?? DateTime.Today.AddDays(-30);
            var to   = toDate   ?? DateTime.Today;

            string sql = @"
                SELECT
                    od.OrderDetailsId,
                    o.OrderId,
                    c.CustomerName,
                    o.OrderDate,
                    p.ProductName,
                    od.OrderQuantity,
                    od.UnitPrice,
                    od.Amount
                FROM Orders o
                JOIN Customer     c  ON o.CustomerId  = c.CustomerId
                JOIN OrderDetails od ON o.OrderId      = od.OrderId
                JOIN Product      p  ON od.ProductId   = p.ProductId
                WHERE o.OrderDate >= @from AND o.OrderDate < @to";

            var fromParam = new System.Data.SqlClient.SqlParameter("@from", from);
            var toParam   = new System.Data.SqlClient.SqlParameter("@to", to.AddDays(1));

            var results = db.Database.SqlQuery<OrderReportRow>(sql, fromParam, toParam).ToList();

            if (!string.IsNullOrWhiteSpace(customerName))
                results = results.Where(r => r.CustomerName.IndexOf(customerName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Order ID,Customer,Order Date,Product,Qty,Unit Price,Amount");
            foreach (var r in results)
            {
                sb.AppendLine($"{r.OrderId}," +
                              $"\"{r.CustomerName}\"," +
                              $"{r.OrderDate:yyyy-MM-dd}," +
                              $"\"{r.ProductName}\"," +
                              $"{r.OrderQuantity}," +
                              $"{r.UnitPrice:F2}," +
                              $"{r.Amount:F2}");
            }

            string fileName = $"OrderReport_{from:yyyyMMdd}_to_{to:yyyyMMdd}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        private class OrderReportRow
        {
            public int OrderDetailsId { get; set; }
            public int OrderId { get; set; }
            public string CustomerName { get; set; }
            public DateTime? OrderDate { get; set; }
            public string ProductName { get; set; }
            public int OrderQuantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
