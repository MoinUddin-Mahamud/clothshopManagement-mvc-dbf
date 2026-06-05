using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    public class ProductCategoryController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();

        public ActionResult Index()
        {
            var categories = db.ProductCategories.Select(c => new ProductCategoryViewModel
            {
                ProductCategoryId = c.ProductCategoryId,
                CategoryName = c.CategoryName,
                CategoryDescription = c.CategoryDescription
            }).ToList();

            return View(categories);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = new ProductCategory
                {
                    CategoryName = model.CategoryName,
                    CategoryDescription = model.CategoryDescription
                };

                db.ProductCategories.Add(category);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Product Category created successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.ProductCategories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var model = new ProductCategoryViewModel
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = db.ProductCategories.Find(model.ProductCategoryId);
                if (category == null)
                {
                    return HttpNotFound();
                }

                category.CategoryName = model.CategoryName;
                category.CategoryDescription = model.CategoryDescription;

                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Product Category updated successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = db.ProductCategories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var model = new ProductCategoryViewModel
            {
                ProductCategoryId = category.ProductCategoryId,
                CategoryName = category.CategoryName,
                CategoryDescription = category.CategoryDescription
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var category = db.ProductCategories.Find(id);
                if (category == null)
                {
                    return HttpNotFound();
                }

                db.ProductCategories.Remove(category);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Product Category deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Cannot delete this category. It may have related products.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreateAjax(ProductCategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var category = new ProductCategory
                    {
                        CategoryName = model.CategoryName,
                        CategoryDescription = model.CategoryDescription
                    };

                    db.ProductCategories.Add(category);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Category created successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Please fill in all required fields correctly." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult EditAjax(int id)
        {
            try
            {
                var category = db.ProductCategories.Find(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" }, JsonRequestBehavior.AllowGet);
                }

                var model = new ProductCategoryViewModel
                {
                    ProductCategoryId = category.ProductCategoryId,
                    CategoryName = category.CategoryName,
                    CategoryDescription = category.CategoryDescription,
                    ProductCount = category.Products?.Count() ?? 0
                };

                return Json(new { success = true, data = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EditAjax(ProductCategoryViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var category = db.ProductCategories.Find(model.ProductCategoryId);
                    if (category == null)
                    {
                        return Json(new { success = false, message = "Category not found" });
                    }

                    category.CategoryName = model.CategoryName;
                    category.CategoryDescription = model.CategoryDescription;

                    db.Entry(category).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Category updated successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Please fill in all required fields correctly." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult DeleteAjax(int id)
        {
            try
            {
                var category = db.ProductCategories.Find(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Category not found" });
                }

                db.ProductCategories.Remove(category);
                db.SaveChanges();

                return Json(new { success = true, message = "Category deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
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
