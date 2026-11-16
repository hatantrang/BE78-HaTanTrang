using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; }
        public int CustomerID { get; set; }
        public string Username { get; set; }

        [Display(Name = "Họ tên")]
        public string CustomerName { get; set; }

        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng giá trị")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Phương thức giao hàng")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        [Required(ErrorMessage = "Vui lòng chọn hình thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Địa chỉ giao hàng")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
        public string ShippingAddress { get; set; }
    }
}
