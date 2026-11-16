using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Services
{
    public class InsufficientStockException : Exception
    {
        public InsufficientStockException(string message) : base(message) { }
    }

    public class StockLine
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }

    public static class StockService
    {
        public static Dictionary<int, int> Merge(IEnumerable<CartItem> items)
        {
            return items
                .GroupBy(i => i.ProductID)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
        }

        public static void RefreshStock(MyStoreEntities db, IEnumerable<CartItem> items)
        {
            var list = items.ToList();
            if (list.Count == 0) { return; }

            var ids = list.Select(i => i.ProductID).Distinct().ToList();
            var stock = db.Products.AsNoTracking()
                .Where(p => ids.Contains(p.ProductID))
                .Select(p => new { p.ProductID, p.ProductStock })
                .ToDictionary(p => p.ProductID, p => p.ProductStock);

            foreach (var item in list)
            {
                int s;
                item.Stock = stock.TryGetValue(item.ProductID, out s) ? s : 0;
            }
        }

        public static List<string> FindProblems(MyStoreEntities db, IEnumerable<CartItem> items)
        {
            var problems = new List<string>();
            var wanted = Merge(items);
            var ids = wanted.Keys.ToList();

            var products = db.Products.AsNoTracking()
                .Where(p => ids.Contains(p.ProductID))
                .Select(p => new { p.ProductID, p.ProductName, p.ProductStock })
                .ToDictionary(p => p.ProductID);

            foreach (var kv in wanted)
            {
                var p = products.ContainsKey(kv.Key) ? products[kv.Key] : null;
                if (p == null)
                {
                    problems.Add("Sản phẩm #" + kv.Key + " không còn tồn tại.");
                }
                else if (p.ProductStock <= 0)
                {
                    problems.Add("\"" + p.ProductName + "\" đã hết hàng.");
                }
                else if (p.ProductStock < kv.Value)
                {
                    problems.Add("\"" + p.ProductName + "\" chỉ còn " + p.ProductStock
                        + " sản phẩm (bạn đang chọn " + kv.Value + ").");
                }
            }
            return problems;
        }

        public static void Deduct(MyStoreEntities db, IEnumerable<CartItem> items)
        {
            foreach (var kv in Merge(items))
            {
                DeductOne(db, kv.Key, kv.Value);
            }
        }

        public static void Restore(MyStoreEntities db, int orderId)
        {
            db.Database.ExecuteSqlCommand(
                "UPDATE p SET p.ProductStock = p.ProductStock + " +
                "(SELECT SUM(d.Quantity) FROM dbo.OrderDetail d WHERE d.OrderID = {0} AND d.ProductID = p.ProductID) " +
                "FROM dbo.Product p " +
                "WHERE p.ProductID IN (SELECT ProductID FROM dbo.OrderDetail WHERE OrderID = {0})",
                orderId);
        }

        public static void DeductOrder(MyStoreEntities db, int orderId)
        {
            var lines = db.Database.SqlQuery<StockLine>(
                "SELECT ProductID, SUM(Quantity) AS Quantity FROM dbo.OrderDetail WHERE OrderID = {0} GROUP BY ProductID",
                orderId).ToList();

            foreach (var line in lines)
            {
                DeductOne(db, line.ProductID, line.Quantity);
            }
        }

        private static void DeductOne(MyStoreEntities db, int productId, int quantity)
        {
            int rows = db.Database.ExecuteSqlCommand(
                "UPDATE dbo.Product SET ProductStock = ProductStock - {0} WHERE ProductID = {1} AND ProductStock >= {0}",
                quantity, productId);

            if (rows == 0)
            {
                string name = db.Products.AsNoTracking()
                    .Where(p => p.ProductID == productId)
                    .Select(p => p.ProductName)
                    .FirstOrDefault();
                throw new InsufficientStockException(
                    "Sản phẩm \"" + (name ?? ("#" + productId)) + "\" không đủ số lượng trong kho.");
            }
        }

        public static bool ChangeOrderStatus(MyStoreEntities db, int orderId, string newStatus,
            string[] allowedFrom, out string error)
        {
            error = null;

            using (var tx = db.Database.BeginTransaction())
            {
                string oldStatus = db.Database
                    .SqlQuery<string>("SELECT OrderStatus FROM dbo.[Order] WHERE OrderID = {0}", orderId)
                    .FirstOrDefault();

                if (oldStatus == null)
                {
                    error = "Không tìm thấy đơn hàng.";
                    return false;
                }
                if (allowedFrom != null && !allowedFrom.Contains(oldStatus))
                {
                    error = "Trạng thái hiện tại của đơn hàng không cho phép thao tác này.";
                    return false;
                }
                if (oldStatus == newStatus)
                {
                    return true;
                }

                int rows = db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.[Order] SET OrderStatus = {0} WHERE OrderID = {1} AND OrderStatus = {2}",
                    newStatus, orderId, oldStatus);
                if (rows != 1)
                {
                    error = "Đơn hàng vừa được cập nhật bởi thao tác khác. Vui lòng tải lại trang.";
                    return false;
                }

                bool wasCancelled = oldStatus == OrderStatuses.Cancelled;
                bool willCancel = newStatus == OrderStatuses.Cancelled;

                if (!wasCancelled && willCancel)
                {
                    Restore(db, orderId);
                }
                else if (wasCancelled && !willCancel)
                {
                    try
                    {
                        DeductOrder(db, orderId);
                    }
                    catch (InsufficientStockException ex)
                    {
                        error = ex.Message + " Không thể mở lại đơn hàng đã hủy.";
                        return false;
                    }
                }

                tx.Commit();
                return true;
            }
        }
    }
}
