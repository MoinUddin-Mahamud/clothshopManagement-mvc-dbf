using MyClothShopManagement.Filters;
using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    [SessionAuthorize]
    public class CustomerController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();
        private const int PageSize = 10;

        public ActionResult Index(string search, int page = 1)
        {
            var query = db.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.CustomerName.Contains(search) || c.ContactNumber.Contains(search));

            int total = query.Count();
            var customers = query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new CustomerViewModel
                {
                    CustomerId     = c.CustomerId,
                    CustomerName   = c.CustomerName,
                    ContactNumber  = c.ContactNumber,
                    ContactAddress = c.ContactAddress,
                    CreatedDate    = c.CreatedDate,
                    OrderCount     = c.Orders.Count(),
                    TotalSpent     = c.Orders.Any() ? c.Orders.Sum(o => o.TotalAmount) : 0
                }).ToList();

            ViewBag.Search      = search;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.TotalCount  = total;
            return View(customers);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var customer = db.Customers.Include(c => c.Orders).FirstOrDefault(c => c.CustomerId == id);
            if (customer == null) return HttpNotFound();

            var vm = new CustomerViewModel
            {
                CustomerId     = customer.CustomerId,
                CustomerName   = customer.CustomerName,
                ContactNumber  = customer.ContactNumber,
                ContactAddress = customer.ContactAddress,
                CreatedDate    = customer.CreatedDate,
                OrderCount     = customer.Orders.Count(),
                TotalSpent     = customer.Orders.Any() ? customer.Orders.Sum(o => o.TotalAmount) : 0
            };

            ViewBag.Orders = customer.Orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(vm);
        }

        [SessionAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View(new CustomerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin")]
        public ActionResult Create(CustomerViewModel model)
        {
            if (db.Customers.Any(c => c.ContactNumber == model.ContactNumber))
                ModelState.AddModelError("ContactNumber", "A customer with this phone number already exists.");

            if (ModelState.IsValid)
            {
                var customer = new Customer
                {
                    CustomerName   = model.CustomerName,
                    ContactNumber  = model.ContactNumber,
                    ContactAddress = model.ContactAddress,
                    CreatedDate    = DateTime.Now
                };
                db.Customers.Add(customer);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            var vm = new CustomerViewModel
            {
                CustomerId     = customer.CustomerId,
                CustomerName   = customer.CustomerName,
                ContactNumber  = customer.ContactNumber,
                ContactAddress = customer.ContactAddress,
                CreatedDate    = customer.CreatedDate
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerViewModel model)
        {
            if (db.Customers.Any(c => c.ContactNumber == model.ContactNumber && c.CustomerId != model.CustomerId))
                ModelState.AddModelError("ContactNumber", "Another customer already has this phone number.");

            if (ModelState.IsValid)
            {
                var customer = db.Customers.Find(model.CustomerId);
                if (customer == null) return HttpNotFound();

                customer.CustomerName   = model.CustomerName;
                customer.ContactNumber  = model.ContactNumber;
                customer.ContactAddress = model.ContactAddress;

                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Customer updated successfully!";
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [SessionAuthorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();

            var vm = new CustomerViewModel
            {
                CustomerId     = customer.CustomerId,
                CustomerName   = customer.CustomerName,
                ContactNumber  = customer.ContactNumber,
                ContactAddress = customer.ContactAddress,
                OrderCount     = customer.Orders.Count()
            };
            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SessionAuthorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var customer = db.Customers.Include(c => c.Orders).FirstOrDefault(c => c.CustomerId == id);
                if (customer == null) return HttpNotFound();

                if (customer.Orders.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete this customer — they have existing orders.";
                    return RedirectToAction("Index");
                }

                db.Customers.Remove(customer);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting customer: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
