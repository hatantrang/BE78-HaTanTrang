using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Services;
using PagedList;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Controllers
{
    public class CartController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        public ActionResult Index(int? page)
        {
            var cart = GetCartService().GetCart();

            if (cart.Items.Any())
            {
                StockService.RefreshStock(db, cart.Items);

                var categoryNames = cart.Items.Select(i => i.Category).Distinct().ToList();
                var inCartIds = cart.Items.Select(i => i.ProductID).ToList();

                var similar = db.Products
                    .Where(p => categoryNames.Contains(p.Category.CategoryName) && !inCartIds.Contains(p.ProductID))
                    .OrderBy(p => p.ProductID);

                cart.PageNumber = page ?? 1;
                cart.SimilarProducts = similar.ToPagedList(cart.PageNumber, cart.PageSize);
            }
            else
            {
                cart.SimilarProducts = null;
            }

            return View(cart);
        }

        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            if (quantity < 1) { quantity = 1; }

            var cart = GetCartService().GetCart();
            int inCart = cart.Items.Where(i => i.ProductID == id).Select(i => i.Quantity).FirstOrDefault();

            if (product.ProductStock <= 0)
            {
                TempData["CartError"] = "\"" + product.ProductName + "\" đã hết hàng.";
                return RedirectToAction("Index");
            }
            if (inCart + quantity > product.ProductStock)
            {
                TempData["CartError"] = "\"" + product.ProductName + "\" chỉ còn " + product.ProductStock
                    + " sản phẩm (trong giỏ của bạn đã có " + inCart + "). Không thể thêm " + quantity + ".";
                return RedirectToAction("Index");
            }

            cart.AddItem(product.ProductID, product.ProductImage,
                product.ProductName, product.ProductPrice, quantity, product.Category.CategoryName);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int id)
        {
            GetCartService().GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetCartService().GetCart();
            var product = db.Products.AsNoTracking().SingleOrDefault(p => p.ProductID == id);

            if (product == null || product.ProductStock <= 0)
            {
                cart.RemoveItem(id);
                TempData["CartError"] = "Sản phẩm đã hết hàng nên được xóa khỏi giỏ.";
            }
            else if (quantity > product.ProductStock)
            {
                cart.UpdateQuantity(id, product.ProductStock);
                TempData["CartError"] = "\"" + product.ProductName + "\" chỉ còn " + product.ProductStock
                    + " sản phẩm, số lượng đã được điều chỉnh.";
            }
            else
            {
                cart.UpdateQuantity(id, quantity);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectForCheckout(int[] selectedIds)
        {
            var cartService = GetCartService();
            var cart = cartService.GetCart();

            var valid = (selectedIds ?? new int[0])
                .Where(id => cart.Items.Any(i => i.ProductID == id))
                .ToList();

            if (valid.Count == 0)
            {
                TempData["CartError"] = "Vui lòng chọn ít nhất một sản phẩm để mua.";
                return RedirectToAction("Index");
            }

            var chosen = cart.Items.Where(i => valid.Contains(i.ProductID)).ToList();
            var stockProblems = StockService.FindProblems(db, chosen);
            if (stockProblems.Count > 0)
            {
                TempData["CartError"] = string.Join(" ", stockProblems);
                return RedirectToAction("Index");
            }

            cartService.SetSelectedIds(valid);
            return RedirectToAction("Checkout", "Order");
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
