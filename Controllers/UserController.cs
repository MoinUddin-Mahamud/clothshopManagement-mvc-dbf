using MyClothShopManagement.Filters;
using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    [SessionAuthorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();

        public ActionResult Index()
        {
            var users = db.Users.Select(u => new UserListViewModel
            {
                UserId   = u.UserId,
                UserName = u.UserName,
                UserRole = u.UserRole,
                IsActive = u.IsActive ?? false
            }).OrderBy(u => u.UserName).ToList();

            return View(users);
        }

        public ActionResult Create()
        {
            ViewBag.Roles = new SelectList(new[] { "Admin", "Staff" });
            return View(new UserViewModel { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel model)
        {
            if (db.Users.Any(u => u.UserName == model.UserName))
                ModelState.AddModelError("UserName", "This username is already taken.");

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.UserName,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    UserRole = model.UserRole,
                    IsActive = model.IsActive
                };
                db.Users.Add(user);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"User '{model.UserName}' created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Roles = new SelectList(new[] { "Admin", "Staff" }, model.UserRole);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleActive(int id)
        {
            var currentUserId = (int)Session["UserId"];
            if (id == currentUserId)
            {
                TempData["ErrorMessage"] = "You cannot deactivate your own account.";
                return RedirectToAction("Index");
            }

            var user = db.Users.Find(id);
            if (user != null)
            {
                user.IsActive = !(user.IsActive ?? false);
                db.SaveChanges();
                TempData["SuccessMessage"] = $"User '{user.UserName}' has been {(user.IsActive == true ? "activated" : "deactivated")}.";
            }
            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            ViewBag.UserName = user.UserName;
            ViewBag.UserId   = user.UserId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(int id, string newPassword, string confirmPassword)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Password must be at least 6 characters.");
                ViewBag.UserName = user.UserName;
                ViewBag.UserId   = user.UserId;
                return View();
            }
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                ViewBag.UserName = user.UserName;
                ViewBag.UserId   = user.UserId;
                return View();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            db.SaveChanges();
            TempData["SuccessMessage"] = $"Password for '{user.UserName}' has been reset successfully!";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
