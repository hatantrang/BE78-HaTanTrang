using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using PagedList;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductsController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, int? categoryId, string stockFilter, string sortOrder, int? page)
        {
            var model = new ProductSearchVM
            {
                SearchTerm = searchTerm,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CategoryID = categoryId,
                StockFilter = stockFilter,
                SortOrder = sortOrder,
                PageNumber = page ?? 1,
                PageSize = 5
            };

            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                                            || p.ProductDescription.Contains(searchTerm)
                                            || p.Category.CategoryName.Contains(searchTerm));
            }

            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
            }

            switch (stockFilter)
            {
                case "in":
                    products = products.Where(p => p.ProductStock > 0);
                    break;
                case "low":
                    products = products.Where(p => p.ProductStock > 0 && p.ProductStock <= ProductSearchVM.LowStockLevel);
                    break;
                case "out":
                    products = products.Where(p => p.ProductStock == 0);
                    break;
                default:
                    model.StockFilter = null;
                    break;
            }

            model.SortOrder = sortOrder;
            switch (sortOrder)
            {
                case "stock_asc":
                    products = products.OrderBy(p => p.ProductStock).ThenBy(p => p.ProductID);
                    break;
                case "stock_desc":
                    products = products.OrderByDescending(p => p.ProductStock).ThenBy(p => p.ProductID);
                    break;
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
                default:
                    products = products.OrderBy(p => p.ProductID);
                    break;
            }

            model.Products = products.ToPagedList(model.PageNumber, model.PageSize);
            ViewBag.Categories = new SelectList(db.Categories.OrderBy(c => c.CategoryName), "CategoryID", "CategoryName", categoryId);

            return View(model);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }
        public ActionResult Inventory()
        {
            var products = db.Products.Include(p => p.Category);
            ViewBag.InStock = products.Where(p => p.ProductStock > 0).OrderBy(p => p.ProductStock).ToList();
            ViewBag.NeedRestock = products.Where(p => p.ProductStock == 0).OrderBy(p => p.ProductName).ToList();
            ViewBag.Sold = db.OrderDetails
                .Where(d => d.Order.OrderStatus != OrderStatuses.Cancelled)
                .GroupBy(d => d.ProductID)
                .Select(g => new { ID = g.Key, Qty = g.Sum(x => x.Quantity) })
                .ToDictionary(x => x.ID, x => x.Qty);
            return View();
        }

        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductStock,ProductImage,UploadImg")] Product product)
        {
            ValidateImage(product);
            ValidateBusinessRules(product);

            if (ModelState.IsValid)
            {

                if (string.IsNullOrEmpty(product.ProductImage))
                {
                    product.ProductImage = "~/Content/images/default_img.png";
                }

                SaveImage(product);
                product.ProductName = product.ProductName.Trim();
                db.Products.Add(product);
                db.SaveChanges();
                TempData["ProductMessage"] = "Đã thêm sản phẩm \"" + product.ProductName + "\".";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Edit(int? id)
        {

            ActionResult result = Details(id);
            ViewResult viewResult = result as ViewResult;
            if (viewResult != null)
            {
                Product product = (Product)viewResult.Model;
                product.OriginalStock = product.ProductStock;
                ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductDescription,ProductPrice,ProductStock,OriginalStock,ProductImage,UploadImg")] Product product)
        {
            ValidateImage(product);
            ValidateBusinessRules(product);

            if (ModelState.IsValid)
            {
                SaveImage(product);
                product.ProductName = product.ProductName.Trim();

                int delta = product.ProductStock - product.OriginalStock;

                using (var tx = db.Database.BeginTransaction())
                {
                    db.Entry(product).State = EntityState.Modified;
                    db.Entry(product).Property(p => p.ProductStock).IsModified = false;
                    db.SaveChanges();

                    if (delta != 0)
                    {
                        int rows = db.Database.ExecuteSqlCommand(
                            "UPDATE dbo.Product SET ProductStock = ProductStock + {0} WHERE ProductID = {1} AND ProductStock + {0} >= 0",
                            delta, product.ProductID);
                        if (rows == 0)
                        {
                            ModelState.AddModelError("ProductStock", "Không thể giảm tồn kho xuống dưới 0: số lượng tồn vừa thay đổi do có đơn hàng mới. Vui lòng mở lại trang sửa.");
                            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
                            return View(product);
                        }
                    }
                    tx.Commit();
                }

                TempData["ProductMessage"] = "Đã cập nhật sản phẩm \"" + product.ProductName + "\".";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        public ActionResult Delete(int? id)
        {
            return Details(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Không thể xóa sản phẩm này vì đã có trong đơn hàng.");
                return View("Delete", product);
            }
            return RedirectToAction("Index");
        }

        private const int MaxImageBytes = 2 * 1024 * 1024;
        private void ValidateImage(Product product)
        {
            if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
            {
                string ext = Path.GetExtension(product.UploadImg.FileName).ToLower();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif")
                {
                    ModelState.AddModelError("UploadImg", "Chỉ chấp nhận các định dạng ảnh PNG, JPG và GIF");
                }
                if (product.UploadImg.ContentLength > MaxImageBytes)
                {
                    ModelState.AddModelError("UploadImg", "Ảnh tối đa 2 MB");
                }
            }
        }

        private void ValidateBusinessRules(Product product)
        {
            if (!db.Categories.Any(c => c.CategoryID == product.CategoryID))
            {
                ModelState.AddModelError("CategoryID", "Danh mục không tồn tại");
            }

            if (!string.IsNullOrWhiteSpace(product.ProductName))
            {
                string name = product.ProductName.Trim();
                bool duplicate = db.Products.Any(p => p.CategoryID == product.CategoryID
                                                   && p.ProductName == name
                                                   && p.ProductID != product.ProductID);
                if (duplicate)
                {
                    ModelState.AddModelError("ProductName", "Danh mục này đã có sản phẩm cùng tên");
                }
            }
        }

        private void SaveImage(Product product)
        {
            if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
            {
                string filename = Guid.NewGuid().ToString("N").Substring(0, 8) + "_" + Path.GetFileName(product.UploadImg.FileName);
                string savePath = "~/Content/images/";
                product.ProductImage = savePath + filename;
                product.UploadImg.SaveAs(Path.Combine(Server.MapPath(savePath), filename));
            }
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