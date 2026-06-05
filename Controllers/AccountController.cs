using MyClothShopManagement.Models;
using MyClothShopManagement.Models.ViewModels;
using System.Linq;
using System.Web.Mvc;

namespace MyClothShopManagement.Controllers
{
    public class AccountController : Controller
    {
        private MyClothShopDBEntities1 db = new MyClothShopDBEntities1();

        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Dashboard");

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = db.Users.FirstOrDefault(u => u.UserName == model.UserName && (u.IsActive ?? false));

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            Session["UserId"]   = user.UserId;
            Session["UserName"] = user.UserName;
            Session["UserRole"] = user.UserRole;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(model);

            int userId = (int)Session["UserId"];
            var user = db.Users.Find(userId);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.Password))
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction("Index", "Dashboard");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
