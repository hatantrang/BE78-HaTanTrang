using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Services
{
    public static class OrderService
    {
        public static List<CartItem> GetCheckoutItems(CartService cartService)
        {
            var cart = cartService.GetCart();
            var selected = cartService.GetSelectedIds();

            IEnumerable<CartItem> items = cart.Items;
            if (selected != null && selected.Count > 0)
            {
                items = items.Where(i => selected.Contains(i.ProductID));
            }
            return items.ToList();
        }

        public static List<CartItem> RefreshPrices(MyStoreEntities db, List<CartItem> items)
        {
            var ids = items.Select(i => i.ProductID).ToList();
            var products = db.Products.Where(p => ids.Contains(p.ProductID)).ToDictionary(p => p.ProductID);

            var result = new List<CartItem>();
            foreach (var item in items)
            {
                Product product;
                if (!products.TryGetValue(item.ProductID, out product)) { continue; }
                item.UnitPrice = product.ProductPrice;
                item.ProductName = product.ProductName;
                item.Stock = product.ProductStock;
                result.Add(item);
            }
            return result;
        }

        public static List<string> StockProblems(MyStoreEntities db, IEnumerable<CartItem> items)
        {
            return StockService.FindProblems(db, items);
        }

        public static void StatusFor(string paymentMethod, out string paymentStatus, out string orderStatus)
        {
            switch (paymentMethod)
            {
                
                case PaymentMethods.PayLater:
                    paymentStatus = PaymentStatuses.Pending;
                    orderStatus = OrderStatuses.AwaitingPayment;
                    break;
                default: // cash on delivery
                    paymentStatus = PaymentStatuses.Unpaid;
                    orderStatus = OrderStatuses.Processing;
                    break;
            }
        }

        public static Order CreateOrder(MyStoreEntities db, Customer customer, IList<CartItem> items,
            string shippingAddress, string deliveryMethod, string paymentMethod)
        {
            string paymentStatus, orderStatus;
            StatusFor(paymentMethod, out paymentStatus, out orderStatus);

            var order = new Order
            {
                CustomerID = customer.CustomerID,
                OrderDate = DateTime.Now,
                TotalAmount = items.Sum(i => i.TotalPrice),
                PaymentStatus = paymentStatus,
                OrderStatus = orderStatus,
                ShippingAddress = shippingAddress,
                DeliveryMethod = deliveryMethod,
                PaymentMethod = paymentMethod,
                OrderDetails = items.Select(i => new OrderDetail
                {
                    ProductID = i.ProductID,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            using (var tx = db.Database.BeginTransaction())
            {
                StockService.Deduct(db, items);
                db.Orders.Add(order);
                db.SaveChanges();
                tx.Commit();
            }
            return order;
        }

        public static void RemovePurchased(CartService cartService, IEnumerable<CartItem> bought)
        {
            cartService.GetCart().RemoveItems(bought.Select(i => i.ProductID).ToList());
            cartService.SetSelectedIds(null);
        }
    }


}
