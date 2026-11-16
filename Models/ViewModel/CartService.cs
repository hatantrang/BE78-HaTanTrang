using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class CartService
    {
        public const string CartKey = "Cart";
        public const string SelectedKey = "CheckoutProductIds";

        private readonly HttpSessionStateBase session;

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public Cart GetCart()
        {
            var cart = session[CartKey] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session[CartKey] = cart;
            }
            return cart;
        }

        public void ClearCart()
        {
            session[CartKey] = null;
            session[SelectedKey] = null;
        }

        public List<int> GetSelectedIds()
        {
            return session[SelectedKey] as List<int>;
        }

        public void SetSelectedIds(IEnumerable<int> ids)
        {
            session[SelectedKey] = ids == null ? null : ids.Distinct().ToList();
        }
    }
}
