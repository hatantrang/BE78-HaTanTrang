using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BE05_06.Controllers
{
    // [Authorize(Roles = "Admin")] // Bật dòng này nếu có phân quyền Admin
    public class AdminOrderController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Admin/AdminOrder/Index
        public ActionResult Index(string searchName, string paymentStatus, DateTime? orderDate)
        {
            var orders = db.Orders.Include("Customer").AsQueryable();

            // 1. Tìm theo tên khách hàng
            if (!string.IsNullOrWhiteSpace(searchName))
            {
                orders = orders.Where(o => o.Customer.CustomerName.Contains(searchName));
            }

            // 2. Lọc theo trạng thái thanh toán (hoặc trạng thái đơn hàng tùy vào cột trong DB của bạn)
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                orders = orders.Where(o => o.PaymentMethod == paymentStatus || o.OrderStatus == paymentStatus);
            }

            // 3. Lọc theo ngày đặt hàng
            if (orderDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.Year == orderDate.Value.Year &&
                                           o.OrderDate.Month == orderDate.Value.Month &&
                                           o.OrderDate.Day == orderDate.Value.Day);
            }

            var model = orders.OrderByDescending(o => o.OrderDate).ToList();
            return View(model);
        }

        // GET: Admin/AdminOrder/Details/5
        public ActionResult Details(int id)
        {
            var order = db.Orders.Include("OrderDetails.Product")
                                 .Include("Customer")
                                 .SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
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