using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using PagedList;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Controllers
{
    public class HomeController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.Where(p => p.ProductStock > 0);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                p.ProductDescription.Contains(searchTerm) ||
                                p.Category.CategoryName.Contains(searchTerm));
            }

            int pageNumber = page ?? 1;
            int pageSize = 6;

            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            model.NewProducts = products.OrderBy(p => p.OrderDetails.Count()).ThenBy(p => p.ProductID).Take(20).ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        public ActionResult ProductList(int? id, string searchTerm, string sortOrder, decimal? minPrice, decimal? maxPrice, bool inStockOnly = false, int? page = null)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }

            var products = db.Products.Where(p => p.CategoryID == id.Value && p.ProductStock > 0);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }
            if (inStockOnly)
            {
                products = products.Where(p => p.ProductStock > 0);
            }

            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                case "bestseller":
                    products = products.OrderByDescending(p => p.OrderDetails.Count()).ThenBy(p => p.ProductID);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductID);
                    break;
            }

            var model = new ProductListVM
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStockOnly = inStockOnly,
                PageNumber = page ?? 1,
                PageSize = 8
            };
            model.Products = products.ToPagedList(model.PageNumber, model.PageSize);

            return View(model);
        }

        [ChildActionOnly]
        public PartialViewResult CategoryMenu()
        {
            var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
            return PartialView("_CategoriesPV", categories);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }

            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();

            ProductDetailsVM model = new ProductDetailsVM();

            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;
            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).ThenBy(p => p.ProductID).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue && quantity.Value > 0)
            {
                model.quantity = quantity.Value;
            }
            if (pro.ProductStock > 0 && model.quantity > pro.ProductStock)
            {
                model.quantity = pro.ProductStock;            }
            model.estimatedValue = model.quantity * pro.ProductPrice;

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
