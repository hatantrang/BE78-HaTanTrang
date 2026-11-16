using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Filters
{
    public static class AdminAuth
    {
        public const string Role = "A";

        public const string SessionKey = "AdminUser";
    }

    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var cache = filterContext.HttpContext.Response.Cache;
            cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            cache.SetNoStore();
            cache.SetExpires(DateTime.UtcNow.AddDays(-1));

            if (filterContext.HttpContext.Session[AdminAuth.SessionKey] == null)
            {
                string returnUrl = filterContext.HttpContext.Request.RawUrl;
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        area = "Admin",
                        controller = "Auth",
                        action = "Login",
                        returnUrl = returnUrl
                    }));
            }
        }
    }
}
