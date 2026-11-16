using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();

        public IEnumerable<CartItem> Items => items;

        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;

        public PagedList.IPagedList<Product> SimilarProducts { get; set; }

        public int Count => items.Count;

        public void AddItem(int productId, string productImage, string productName, decimal unitPrice, int quantity, string category)
        {
            if (quantity < 1) { quantity = 1; }

            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);
            if (existingItem == null)
            {
                items.Add(new CartItem
                {
                    ProductID = productId,
                    ProductImage = productImage,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Category = category
                });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public void RemoveItems(IEnumerable<int> productIds)
        {
            var ids = new HashSet<int>(productIds);
            items.RemoveAll(i => ids.Contains(i.ProductID));
        }

        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        public decimal TotalValue(IEnumerable<int> productIds)
        {
            var ids = new HashSet<int>(productIds);
            return items.Where(i => ids.Contains(i.ProductID)).Sum(i => i.TotalPrice);
        }

        public void Clear()
        {
            items.Clear();
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                item.Quantity = quantity < 1 ? 1 : quantity;
            }
        }
    }
}
