using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    public class OrderController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();

        private const int PageSize = 15;

        public ActionResult Index(string search, int page = 1)
        {
            var query = db.Orders.Include(s => s.Customer).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Customer.CustomerName.Contains(search));

            int total = query.Count();
            var orders = query
                .OrderByDescending(s => s.OrderDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(s => new SalesMasterViewModel
                {
                    SaleId = s.OrderId,
                    OrderId = s.OrderId,
                    CustomerName = s.Customer.CustomerName,
                    Phone = s.Customer.ContactNumber,
                    Address = s.Customer.ContactAddress,
                    OrderDate = (DateTime)s.OrderDate,
                    TotalAmount = s.TotalAmount,
                    CustomerId = s.CustomerId
                }).ToList();

            ViewBag.Search      = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)System.Math.Ceiling((double)total / PageSize);
            ViewBag.TotalCount  = total;
            return View(orders);
        }

        public ActionResult Create()
        {
            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(new SalesMasterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SalesMasterViewModel model, string OrderDetailsListJson)
        {
            if (!string.IsNullOrEmpty(OrderDetailsListJson))
            {
                model.SalesDetailsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalesDetailsViewModel>>(OrderDetailsListJson);
            }

            if (model.OrderDate == default(DateTime))
            {
                model.OrderDate = DateTime.Now;
            }

            ModelState.Remove("OrderDate");

            if (ModelState.IsValid || model.SalesDetailsList != null)
            {
                if (model.SalesDetailsList == null || !model.SalesDetailsList.Any())
                {
                    TempData["ErrorMessage"] = "Please add at least one product to the order.";
                    ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                    return View(model);
                }

                using (var transaction = new TransactionScope())
                {
                    try
                    {
                        foreach (var item in model.SalesDetailsList)
                        {
                            var product = db.Products.Find(item.ProductId ?? 0);
                            if (product.AvailableQuantity < item.Quantity)
                            {
                                TempData["ErrorMessage"] = $"Insufficient stock for {product.ProductName}. Available: {product.AvailableQuantity}";
                                ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                                return View(model);
                            }
                        }

                        var customer = db.Customers.FirstOrDefault(c => c.ContactNumber == model.Phone);
                        if (customer == null)
                        {
                            customer = new Customer
                            {
                                CustomerName = model.CustomerName,
                                ContactNumber = model.Phone,
                                ContactAddress = model.Address
                            };
                            db.Customers.Add(customer);
                            db.SaveChanges();
                        }
                        else
                        {
                            customer.CustomerName = model.CustomerName;
                            customer.ContactAddress = model.Address;
                            db.Entry(customer).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        var order = new Order
                        {
                            CustomerId = customer.CustomerId,
                            OrderDate = DateTime.Now,
                            TotalAmount = model.SalesDetailsList.Sum(d => d.Amount)
                        };
                        db.Orders.Add(order);
                        db.SaveChanges();

                        foreach (var item in model.SalesDetailsList)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductCategoryId = item.ProductCategoryId ?? 0,
                                ProductId = item.ProductId ?? 0,
                                OrderQuantity = item.OrderQuantity ?? item.Quantity,
                                OrderUnit = item.OrderUnit ?? item.Unit,
                                UnitPrice = item.UnitPrice,
                                Amount = item.Amount
                            };
                            db.OrderDetails.Add(orderDetail);
                        }

                        db.SaveChanges();

                        foreach (var item in model.SalesDetailsList)
                        {
                            var product = db.Products.Find(item.ProductId ?? 0);
                            if (product != null)
                            {
                                product.AvailableQuantity -= (item.OrderQuantity ?? item.Quantity);
                                db.Entry(product).State = EntityState.Modified;
                            }
                        }

                        db.SaveChanges();
                        transaction.Complete();

                        TempData["SuccessMessage"] = "Order placed successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Error placing order: " + ex.Message;
                        ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                        return View(model);
                    }
                }
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new SalesMasterViewModel
            {
                SaleId = order.OrderId,
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.CustomerName,
                Phone = order.Customer.ContactNumber,
                Address = order.Customer.ContactAddress,
                OrderDate = (DateTime)order.OrderDate,
                TotalAmount = order.TotalAmount,
                SalesDetailsList = order.OrderDetails.Select(od => new SalesDetailsViewModel
                {
                    OrderDetailsId = od.OrderDetailsId,
                    OrderId = od.OrderId,
                    ProductCategoryId = od.ProductCategoryId,
                    ProductId = od.ProductId,
                    OrderQuantity = od.OrderQuantity,
                    Quantity = od.OrderQuantity,
                    OrderUnit = od.OrderUnit,
                    Unit = od.OrderUnit,
                    UnitPrice = od.UnitPrice,
                    Amount = od.Amount,
                    ProductName = od.Product.ProductName,
                    CategoryName = od.ProductCategory.CategoryName
                }).ToList()
            };

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SalesMasterViewModel model, string OrderDetailsListJson)
        {
            if (!string.IsNullOrEmpty(OrderDetailsListJson))
            {
                model.SalesDetailsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalesDetailsViewModel>>(OrderDetailsListJson);
            }

            if (ModelState.IsValid)
            {
                if (model.SalesDetailsList == null || !model.SalesDetailsList.Any())
                {
                    TempData["ErrorMessage"] = "Please add at least one product to the order.";
                    ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                    return View(model);
                }

                using (var transaction = new TransactionScope())
                {
                    try
                    {
                        var order = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderId == model.SaleId);
                        if (order == null)
                        {
                            return HttpNotFound();
                        }

                        foreach (var oldDetail in order.OrderDetails.ToList())
                        {
                            var product = db.Products.Find(oldDetail.ProductId);
                            product.AvailableQuantity += oldDetail.OrderQuantity;
                            db.Entry(product).State = EntityState.Modified;
                        }

                        foreach (var item in model.SalesDetailsList)
                        {
                            var product = db.Products.Find(item.ProductId ?? 0);
                            int qty = item.OrderQuantity ?? item.Quantity;
                            if (product.AvailableQuantity < qty)
                            {
                                foreach (var oldDetail in order.OrderDetails)
                                {
                                    var prod = db.Products.Find(oldDetail.ProductId);
                                    prod.AvailableQuantity -= oldDetail.OrderQuantity;
                                    db.Entry(prod).State = EntityState.Modified;
                                }
                                db.SaveChanges();

                                TempData["ErrorMessage"] = $"Insufficient stock for {product.ProductName}. Available: {product.AvailableQuantity}";
                                ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                                return View(model);
                            }
                        }

                        var customer = db.Customers.Find(order.CustomerId);
                        customer.CustomerName = model.CustomerName;
                        customer.ContactNumber = model.Phone;
                        customer.ContactAddress = model.Address;
                        db.Entry(customer).State = EntityState.Modified;

                        db.OrderDetails.RemoveRange(order.OrderDetails);
                        db.SaveChanges();

                        foreach (var item in model.SalesDetailsList)
                        {
                            int qty = item.OrderQuantity ?? item.Quantity;
                            string unit = item.OrderUnit ?? item.Unit;
                            
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductCategoryId = item.ProductCategoryId ?? 0,
                                ProductId = item.ProductId ?? 0,
                                OrderQuantity = qty,
                                OrderUnit = unit,
                                UnitPrice = item.UnitPrice,
                                Amount = item.Amount
                            };
                            db.OrderDetails.Add(orderDetail);
                        }

                        db.SaveChanges();

                        foreach (var item in model.SalesDetailsList)
                        {
                            int qty = item.OrderQuantity ?? item.Quantity;
                            var product = db.Products.Find(item.ProductId ?? 0);
                            if (product != null)
                            {
                                product.AvailableQuantity -= qty;
                                db.Entry(product).State = EntityState.Modified;
                            }
                        }

                        order.TotalAmount = model.SalesDetailsList.Sum(d => d.Amount);
                        db.Entry(order).State = EntityState.Modified;

                        db.SaveChanges();
                        transaction.Complete();

                        TempData["SuccessMessage"] = "Order updated successfully!";
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = "Error updating order: " + ex.Message;
                        ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
                        return View(model);
                    }
                }
            }

            ViewBag.ProductCategories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var order = db.Orders.Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new SalesMasterViewModel
            {
                SaleId = order.OrderId,
                OrderId = order.OrderId,
                CustomerName = order.Customer.CustomerName,
                Phone = order.Customer.ContactNumber,
                Address = order.Customer.ContactAddress,
                OrderDate = (DateTime)order.OrderDate,
                TotalAmount = order.TotalAmount,
                SalesDetailsList = order.OrderDetails.Select(od => new SalesDetailsViewModel
                {
                    ProductName = od.Product.ProductName,
                    CategoryName = od.ProductCategory.CategoryName,
                    OrderQuantity = od.OrderQuantity,
                    OrderUnit = od.OrderUnit,
                    UnitPrice = od.UnitPrice,
                    Amount = od.Amount
                }).ToList()
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (var transaction = new TransactionScope())
            {
                try
                {
                    var order = db.Orders.Include(o => o.OrderDetails).FirstOrDefault(o => o.OrderId == id);
                    if (order == null)
                    {
                        return HttpNotFound();
                    }

                    foreach (var detail in order.OrderDetails)
                    {
                        var product = db.Products.Find(detail.ProductId);
                        product.AvailableQuantity += detail.OrderQuantity;
                        db.Entry(product).State = EntityState.Modified;
                    }

                    db.Orders.Remove(order);
                    db.SaveChanges();
                    transaction.Complete();

                    TempData["SuccessMessage"] = "Order deleted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Error deleting order: " + ex.Message;
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpGet]
        public JsonResult GetProductDetails(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                success = true,
                size = product.Size,
                color = product.Color,
                unitPrice = product.UnitPrice,
                availableQuantity = product.AvailableQuantity
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = db.Products
                    .Where(p => p.ProductCategoryId == categoryId)
                    .ToList()
                    .Select(p => new
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Size = p.Size,
                        Color = p.Color,
                        UnitPrice = p.UnitPrice,
                        AvailableQuantity = p.AvailableQuantity
                    })
                    .ToList();

                return Json(products, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}