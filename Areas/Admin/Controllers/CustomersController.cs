using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Services;
using PagedList;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CustomersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            var model = new CustomerSearchVM();
            var customers = db.Customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                model.SearchTerm = searchTerm;
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm)
                                              || c.CustomerPhone.Contains(searchTerm)
                                              || c.CustomerEmail.Contains(searchTerm)
                                              || c.Username.Contains(searchTerm));
            }

            model.SortOrder = sortOrder;
            switch (sortOrder)
            {
                case "name_asc":
                    customers = customers.OrderBy(c => c.CustomerName);
                    break;
                case "name_desc":
                    customers = customers.OrderByDescending(c => c.CustomerName);
                    break;
                case "email_asc":
                    customers = customers.OrderBy(c => c.CustomerEmail);
                    break;
                case "email_desc":
                    customers = customers.OrderByDescending(c => c.CustomerEmail);
                    break;
                default:
                    customers = customers.OrderBy(c => c.CustomerID);
                    break;
            }

            model.PageNumber = page ?? 1;
            model.PageSize = 5;
            model.Customers = customers.ToPagedList(model.PageNumber, model.PageSize);

            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id);
            if (customer == null)
            {
                return HttpNotFound();
            }

            var model = new CustomerOrdersVM
            {
                Customer = customer,
                Orders = db.Orders
                           .Where(o => o.CustomerID == id.Value)
                           .OrderByDescending(o => o.OrderDate)
                           .ToList()
            };
            return View(model);
        }

        public ActionResult OrderDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            var model = new OrderDetailsVM
            {
                Order = order,
                Customer = db.Customers.Find(order.CustomerID),
                Items = db.OrderDetails
                          .Include(d => d.Product)
                          .Where(d => d.OrderID == id.Value)
                          .ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateOrderStatus(int id, string status)
        {
            if (!db.Orders.AsNoTracking().Any(o => o.OrderID == id))
            {
                return HttpNotFound();
            }
            if (!OrderStatuses.All.Contains(status ?? ""))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string error;
            if (!StockService.ChangeOrderStatus(db, id, status, null, out error))
            {
                TempData["OrderStatusError"] = error;
                return RedirectToAction("OrderDetails", new { id = id });
            }

            if (status == OrderStatuses.Delivered)
            {
                Order order = db.Orders.Find(id);
                if (order != null)
                {
                    db.Entry(order).Reload();                    if (order.PaymentMethod == PaymentMethods.Cash)
                    {
                        order.PaymentStatus = PaymentStatuses.Paid;                        db.SaveChanges();
                    }
                }
            }

            TempData["OrderStatusMessage"] = "Đã cập nhật trạng thái đơn hàng #" + id + ".";
            return RedirectToAction("OrderDetails", new { id = id });
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
