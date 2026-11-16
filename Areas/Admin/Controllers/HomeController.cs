using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using PagedList;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index()
        {
            var counts = db.Products
                           .GroupBy(p => p.CategoryID)
                           .Select(g => new { CategoryID = g.Key, Count = g.Count() })
                           .ToList();

            var stats = db.Categories
                          .ToList()
                          .Select(c => new CategoryStat
                          {
                              Name = c.CategoryName,
                              Count = counts.Where(x => x.CategoryID == c.CategoryID)
                                            .Select(x => x.Count)
                                            .FirstOrDefault()
                          })
                          .ToList();

            var model = new HomeStatsVM
            {
                ProductCount = db.Products.Count(),
                CategoryCount = db.Categories.Count(),
                CustomerCount = db.Customers.Count(),
                OrderCount = db.Orders.Count(),
                Revenue = db.Orders.Where(o => o.OrderStatus != OrderStatuses.Cancelled).Sum(o => (decimal?)o.TotalAmount) ?? 0,
                CategoryStats = stats
            };

            return View(model);
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
