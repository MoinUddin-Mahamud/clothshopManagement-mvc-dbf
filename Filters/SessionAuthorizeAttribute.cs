using System.Web;
using System.Web.Mvc;

namespace MyClothShopManagement.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public string Roles { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            var userId = session["UserId"];

            if (userId == null)
            {
                filterContext.Result = new RedirectResult("~/Account/Login?returnUrl=" +
                    HttpUtility.UrlEncode(filterContext.HttpContext.Request.RawUrl));
                return;
            }

            if (!string.IsNullOrEmpty(Roles))
            {
                var userRole = session["UserRole"] as string;
                bool allowed = false;
                foreach (var role in Roles.Split(','))
                {
                    if (role.Trim().Equals(userRole, System.StringComparison.OrdinalIgnoreCase))
                    {
                        allowed = true;
                        break;
                    }
                }
                if (!allowed)
                {
                    filterContext.Result = new ViewResult { ViewName = "~/Views/Shared/AccessDenied.cshtml" };
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
