using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using MyClothShopManagement.Filters;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    [SessionAuthorize]
    public class ProductController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();
        private const int PageSize = 12;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ActionResult Index(int? categoryId, string searchName, int page = 1)
        {
            var products = db.Products.Include(p => p.ProductCategory).AsQueryable();

            if (categoryId.HasValue && categoryId.Value > 0)
                products = products.Where(p => p.ProductCategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(searchName))
                products = products.Where(p => p.ProductName.Contains(searchName));

            int total = products.Count();
            var productList = products
                .OrderBy(p => p.ProductName)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(p => new MyClothProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Size = p.Size,
                    Color = p.Color,
                    UnitPrice = p.UnitPrice,
                    AvailableQuantity = p.AvailableQuantity,
                    ProductImage = p.ProductImage,
                    ProductCategoryId = p.ProductCategoryId,
                    CategoryName = p.ProductCategory.CategoryName,
                    IsActive = p.IsActive ?? false
                }).ToList();

            ViewBag.Categories = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchName = searchName;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling((double)total / PageSize);
            ViewBag.TotalCount  = total;

            return View(productList);
        }

        public ActionResult Create()
        {
            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MyClothProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var product = new Product
                    {
                        ProductName = model.ProductName,
                        Size = model.Size,
                        Color = model.Color,
                        UnitPrice = model.UnitPrice,
                        AvailableQuantity = model.AvailableQuantity,
                        IsActive = model.IsActive,
                        ProductCategoryId = model.ProductCategoryId
                    };

                    if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                    {
                        string imageError = ValidateImageFile(model.ImageFile);
                        if (imageError != null)
                        {
                            ModelState.AddModelError("ImageFile", imageError);
                            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
                            return View(model);
                        }

                        product.ProductImage = SaveImageFile(model.ImageFile);
                    }

                    db.Products.Add(product);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while creating the product: " + ex.Message);
                }
            }

            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var model = new MyClothProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Size = product.Size,
                Color = product.Color,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
                IsActive = product.IsActive ?? false
            };

            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MyClothProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var product = db.Products.Find(model.ProductId);
                    if (product == null) return HttpNotFound();

                    product.ProductName = model.ProductName;
                    product.Size = model.Size;
                    product.Color = model.Color;
                    product.UnitPrice = model.UnitPrice;
                    product.AvailableQuantity = model.AvailableQuantity;
                    product.ProductCategoryId = model.ProductCategoryId;
                    product.IsActive = model.IsActive;

                    if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
                    {
                        string imageError = ValidateImageFile(model.ImageFile);
                        if (imageError != null)
                        {
                            ModelState.AddModelError("ImageFile", imageError);
                            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
                            return View(model);
                        }

                        DeleteImageFile(product.ProductImage);
                        product.ProductImage = SaveImageFile(model.ImageFile);
                    }

                    db.Entry(product).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while updating the product: " + ex.Message);
                }
            }

            ViewBag.ProductCategoryId = new SelectList(db.ProductCategories, "ProductCategoryId", "CategoryName", model.ProductCategoryId);
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Include(p => p.ProductCategory).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();

            var model = new MyClothProductViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                UnitPrice = product.UnitPrice,
                AvailableQuantity = product.AvailableQuantity,
                ProductImage = product.ProductImage,
                ProductCategoryId = product.ProductCategoryId,
                CategoryName = product.ProductCategory.CategoryName
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null) return HttpNotFound();

                var relatedOrderDetails = db.OrderDetails.Where(od => od.ProductId == id).ToList();
                db.OrderDetails.RemoveRange(relatedOrderDetails);
                db.SaveChanges();

                DeleteImageFile(product.ProductImage);

                db.Products.Remove(product);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Product deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the product: " + ex.Message;
                return RedirectToAction("Delete", new { id = id });
            }
        }

        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                    return Json(new { success = false, message = "Product not found" });

                product.IsActive = !(product.IsActive ?? false);
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, isActive = product.IsActive });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private string ValidateImageFile(HttpPostedFileBase file)
        {
            if (file.ContentLength > 5 * 1024 * 1024)
                return "File size cannot exceed 5MB.";

            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!Array.Exists(AllowedExtensions, e => e == ext))
                return "Only image files are allowed (.jpg, .jpeg, .png, .gif, .webp).";

            byte[] header = new byte[8];
            file.InputStream.Read(header, 0, 8);
            file.InputStream.Position = 0;

            bool isImage = (header[0] == 0xFF && header[1] == 0xD8) // JPEG
                        || (header[0] == 0x89 && header[1] == 0x50) // PNG
                        || (header[0] == 0x47 && header[1] == 0x49) // GIF
                        || (header[0] == 0x52 && header[1] == 0x49); // WEBP (RIFF)

            if (!isImage)
                return "The uploaded file is not a valid image.";

            return null;
        }

        private string SaveImageFile(HttpPostedFileBase file)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();
            string fileName = Guid.NewGuid() + ext;
            string dir = Server.MapPath("~/Images");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            file.SaveAs(Path.Combine(dir, fileName));
            return fileName;
        }

        private void DeleteImageFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            string path = Path.Combine(Server.MapPath("~/Images"), fileName);
            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); } catch { }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
