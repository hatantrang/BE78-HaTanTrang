using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Controllers
{
    public class OrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        private Customer GetCurrentCustomer()
        {
            string username = User.Identity.Name;
            return db.Customers.SingleOrDefault(c => c.Username == username);
        }


        [Authorize]
        public ActionResult Checkout()
        {
            var cartService = GetCartService();
            var items = OrderService.RefreshPrices(db, OrderService.GetCheckoutItems(cartService));
            if (items.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockProblems = OrderService.StockProblems(db, items);
            if (stockProblems.Count > 0)
            {
                TempData["CartError"] = "Không thể đặt hàng: " + string.Join(" ", stockProblems);
                return RedirectToAction("Index", "Cart");
            }

            var model = BuildCheckoutVM(customer, items);
            model.ShippingAddress = customer.CustomerAddress;
            model.DeliveryMethod = DeliveryMethods.Standard;
            model.PaymentMethod = PaymentMethods.Cash;
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cartService = GetCartService();
            var items = OrderService.RefreshPrices(db, OrderService.GetCheckoutItems(cartService));
            if (items.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var stockProblems = OrderService.StockProblems(db, items);
            if (stockProblems.Count > 0)
            {
                TempData["CartError"] = "Không thể đặt hàng: " + string.Join(" ", stockProblems);
                return RedirectToAction("Index", "Cart");
            }

            var server = BuildCheckoutVM(customer, items);
            model.CartItems = server.CartItems;
            model.TotalAmount = server.TotalAmount;
            model.CustomerID = server.CustomerID;
            model.Username = server.Username;
            model.CustomerName = server.CustomerName;
            model.CustomerPhone = server.CustomerPhone;
            model.OrderDate = server.OrderDate;

            if (!DeliveryMethods.All.Contains(model.DeliveryMethod ?? ""))
            {
                ModelState.AddModelError("DeliveryMethod", "Hình thức giao hàng không hợp lệ");
            }
            if (!PaymentMethods.All.Contains(model.PaymentMethod ?? ""))
            {
                ModelState.AddModelError("PaymentMethod", "Hình thức thanh toán không hợp lệ");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

           

            Order order;
            try
            {
                order = OrderService.CreateOrder(db, customer, items,
                    model.ShippingAddress.Trim(), model.DeliveryMethod, model.PaymentMethod);
            }
            catch (InsufficientStockException ex)
            {
                TempData["CartError"] = "Không thể đặt hàng: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }

            OrderService.RemovePurchased(cartService, items);
            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        private static CheckoutVM BuildCheckoutVM(Customer customer, List<CartItem> items)
        {
            return new CheckoutVM
            {
                CartItems = items,
                TotalAmount = items.Sum(i => i.TotalPrice),
                OrderDate = DateTime.Now,
                CustomerID = customer.CustomerID,
                Username = customer.Username,
                CustomerName = customer.CustomerName,
                CustomerPhone = customer.CustomerPhone
            };
        }


        [Authorize]
        public ActionResult OrderSuccess(int id)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Include("OrderDetails.Product")
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }


        [Authorize]
        public ActionResult MyOrder(string tab, string searchTerm)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var mine = db.Orders.Where(o => o.CustomerID == customer.CustomerID);

            var counts = mine.GroupBy(o => o.OrderStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var model = new MyOrderVM
            {
                Tab = OrderStatuses.All.Contains(tab ?? "") ? tab : null,
                SearchTerm = searchTerm,
                TotalCount = counts.Sum(c => c.Count),
                StatusCounts = OrderStatuses.All.ToDictionary(
                    s => s,
                    s => counts.Where(c => c.Status == s).Select(c => c.Count).FirstOrDefault())
            };

            var query = db.Orders.Include("OrderDetails.Product")
                .Where(o => o.CustomerID == customer.CustomerID);

            if (model.Tab != null)
            {
                string status = model.Tab;
                query = query.Where(o => o.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim();
                int orderId;
                bool isId = int.TryParse(term.TrimStart('#'), out orderId);
                query = query.Where(o => (isId && o.OrderID == orderId)
                                      || o.OrderDetails.Any(d => d.Product.ProductName.Contains(term)));
            }

            model.Orders = query.OrderByDescending(o => o.OrderDate).ToList();
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult CancelOrder(int id)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.AsNoTracking()
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (order.OrderStatus == OrderStatuses.AwaitingPayment || order.OrderStatus == OrderStatuses.Processing)
            {
                string error;
                bool ok = StockService.ChangeOrderStatus(db, order.OrderID, OrderStatuses.Cancelled,
                    new[] { OrderStatuses.AwaitingPayment, OrderStatuses.Processing }, out error);

                if (ok)
                {
                    TempData["OrderMessage"] = "Đã hủy đơn hàng #" + order.OrderID + ". Sản phẩm đã được hoàn lại kho.";
                }
                else
                {
                    TempData["OrderError"] = error ?? "Đơn hàng này không thể hủy.";
                }
            }
            else
            {
                TempData["OrderError"] = "Đơn hàng này không thể hủy.";
            }
            return RedirectToAction("MyOrder");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Reorder(int id)
        {
            var customer = GetCurrentCustomer();
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = db.Orders.Include("OrderDetails.Product.Category")
                .SingleOrDefault(o => o.OrderID == id && o.CustomerID == customer.CustomerID);
            if (order == null)
            {
                return HttpNotFound();
            }

            var cart = GetCartService().GetCart();
            var skipped = new List<string>();
            foreach (var d in order.OrderDetails)
            {
                if (d.Product == null) { continue; }

                int inCart = cart.Items.Where(i => i.ProductID == d.ProductID).Select(i => i.Quantity).FirstOrDefault();
                int canAdd = Math.Min(d.Quantity, d.Product.ProductStock - inCart);
                if (canAdd <= 0)
                {
                    skipped.Add("\"" + d.Product.ProductName + "\" đã hết hàng hoặc đã đạt số lượng tối đa trong giỏ.");
                    continue;
                }
                if (canAdd < d.Quantity)
                {
                    skipped.Add("\"" + d.Product.ProductName + "\" chỉ thêm được " + canAdd + "/" + d.Quantity + " sản phẩm do kho không đủ.");
                }

                cart.AddItem(d.Product.ProductID, d.Product.ProductImage, d.Product.ProductName,
                    d.Product.ProductPrice, canAdd, d.Product.Category.CategoryName);
            }
            if (skipped.Count > 0)
            {
                TempData["CartError"] = string.Join(" ", skipped);
            }
            return RedirectToAction("Index", "Cart");
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
