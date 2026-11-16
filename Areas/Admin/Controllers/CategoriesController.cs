using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Filters;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class CategoriesController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        public ActionResult Index(string searchTerm)
        {
            var categories = db.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                categories = categories.Where(c => c.CategoryName.Contains(searchTerm.Trim()));
            }
            ViewBag.SearchTerm = searchTerm;
            return View(categories.Include(c => c.Products).OrderBy(c => c.CategoryName).ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Include(c => c.Products).SingleOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            ValidateName(category);
            if (ModelState.IsValid)
            {
                category.CategoryName = category.CategoryName.Trim();
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public ActionResult Edit(int? id)
        {
            return Details(id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            ValidateName(category);
            if (ModelState.IsValid)
            {
                category.CategoryName = category.CategoryName.Trim();
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public ActionResult Delete(int? id)
        {
            return Details(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            try
            {
                db.Categories.Remove(category);
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Không thể xóa danh mục này vì vẫn còn sản phẩm thuộc danh mục.");
                return View("Delete", category);
            }
            return RedirectToAction("Index");
        }

        private void ValidateName(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName)) { return; }

            string name = category.CategoryName.Trim();
            if (db.Categories.Any(c => c.CategoryName == name && c.CategoryID != category.CategoryID))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục đã tồn tại");
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
