using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string status)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.OrderStatus == status);
            }
            ViewBag.Status = status;
            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}