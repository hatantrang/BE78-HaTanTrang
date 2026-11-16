using _25DH110581_Hà_Tấn_Trạng_MyStore.Controllers;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Filters
{
    public static class CustomerSession
    {
        public const string UsernameKey = "Username";
        public const string RoleKey = "UserRole";
        public const string NameKey = "CustomerName";
        public const string CustomerRole = "C";

        public static void Start(HttpSessionStateBase session, string username, string customerName)
        {
            session[UsernameKey] = username;
            session[RoleKey] = CustomerRole;
            session[NameKey] = customerName;
        }

        public static void ClearIdentity(HttpSessionStateBase session)
        {
            session.Remove(UsernameKey);
            session.Remove(RoleKey);
            session.Remove(NameKey);
        }

        public static void End(HttpSessionStateBase session)
        {
            ClearIdentity(session);
            session.Remove(CartService.CartKey);
            session.Remove(CartService.SelectedKey);
            
        }
    }

    public class SessionSyncAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction) { return; }

            var ctx = filterContext.HttpContext;
            var session = ctx.Session;
            if (session == null) { return; }

            bool signedIn = ctx.User != null && ctx.User.Identity != null && ctx.User.Identity.IsAuthenticated;

            if (!signedIn)
            {
                if (session[CustomerSession.UsernameKey] != null)
                {
                    CustomerSession.ClearIdentity(session);
                }
                return;
            }

            string username = ctx.User.Identity.Name;
            string inSession = session[CustomerSession.UsernameKey] as string;

            if (!string.Equals(inSession, username, StringComparison.Ordinal))
            {
                using (var db = new MyStoreEntities())
                {
                    var user = db.Users.AsNoTracking()
                        .FirstOrDefault(u => u.Username == username && u.UserRole == CustomerSession.CustomerRole);

                    if (user == null)
                    {
                        FormsAuthentication.SignOut();
                        CustomerSession.End(session);
                    }
                    else
                    {
                        string name = db.Customers.AsNoTracking()
                            .Where(c => c.Username == username)
                            .Select(c => c.CustomerName)
                            .FirstOrDefault();
                        CustomerSession.Start(session, username, name);
                    }
                }
            }

            var cache = ctx.Response.Cache;
            cache.SetCacheability(HttpCacheability.NoCache);
            cache.SetNoStore();
            cache.SetExpires(DateTime.UtcNow.AddDays(-1));
        }
    }
}
