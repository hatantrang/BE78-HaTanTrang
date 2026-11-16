using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Controllers
{
    public class AccountController : Controller
    {
        private const string CustomerRole = "C";

        private MyStoreEntities db = new MyStoreEntities();


        [HttpGet]
        public ActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string username = model.Username.Trim();

            if (db.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                return View(model);
            }

            var user = new User
            {
                Username = username,
                Password = model.Password,
                UserRole = CustomerRole
            };
            var customer = new Customer
            {
                CustomerName = model.CustomerName.Trim(),
                CustomerEmail = model.CustomerEmail.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                CustomerAddress = model.CustomerAddress.Trim(),
                Username = username
            };

            db.Users.Add(user);
            db.Customers.Add(customer);
            db.SaveChanges();

            TempData["RegisterSuccess"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }


        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                string username = model.Username.Trim();
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.UserRole == CustomerRole);

                if (user != null && string.Equals((user.Password ?? "").TrimEnd(), model.Password, StringComparison.Ordinal))
                {
                    SignIn(user.Username, model.RememberMe);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        public ActionResult Logout()
        {
            CustomerSession.End(Session);
            FormsAuthentication.SignOut();

            if (Session[AdminAuth.SessionKey] == null)
            {
                Session.Abandon();
            }
            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        public ActionResult ProfileInfo()
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return ForceRelogin();
            }
            return View(ToProfile(customer));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileVM model)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return ForceRelogin();
            }

            if (!ModelState.IsValid)
            {
                model.Username = customer.Username;
                model.CustomerName = customer.CustomerName;
                return View("ProfileInfo", model);
            }

            customer.CustomerPhone = model.CustomerPhone.Trim();
            customer.CustomerEmail = model.CustomerEmail.Trim();
            customer.CustomerAddress = model.CustomerAddress.Trim();
            db.SaveChanges();

            TempData["ProfileMessage"] = "Đã cập nhật thông tin tài khoản.";
            return RedirectToAction("ProfileInfo");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)));
                return RedirectToAction("ProfileInfo");
            }

            string username = User.Identity.Name;
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.UserRole == CustomerRole);
            if (user == null)
            {
                return ForceRelogin();
            }

            if (!string.Equals((user.Password ?? "").TrimEnd(), model.CurrentPassword, StringComparison.Ordinal))
            {
                TempData["PasswordError"] = "Mật khẩu hiện tại không đúng.";
                return RedirectToAction("ProfileInfo");
            }

            user.Password = model.NewPassword;
            db.SaveChanges();

            TempData["PasswordMessage"] = "Đã đổi mật khẩu.";
            return RedirectToAction("ProfileInfo");
        }


        private void SignIn(string username, bool remember)
        {
            string name = db.Customers.Where(c => c.Username == username)
                .Select(c => c.CustomerName).FirstOrDefault();

            CustomerSession.Start(Session, username, name);
            FormsAuthentication.SetAuthCookie(username, remember);
        }

        private Customer GetCurrentCustomer()
        {
            string username = User.Identity.Name;
            return db.Customers.SingleOrDefault(c => c.Username == username);
        }

        private static ProfileVM ToProfile(Customer c)
        {
            return new ProfileVM
            {
                Username = c.Username,
                CustomerName = c.CustomerName,
                CustomerPhone = c.CustomerPhone,
                CustomerEmail = c.CustomerEmail,
                CustomerAddress = c.CustomerAddress
            };
        }

        private ActionResult ForceRelogin()
        {
            FormsAuthentication.SignOut();
            CustomerSession.End(Session);
            return RedirectToAction("Login");
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
