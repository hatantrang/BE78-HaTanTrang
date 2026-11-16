using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class ProfileVM
    {
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Display(Name = "Họ tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [RegularExpression(ValidationPatterns.VnPhone, ErrorMessage = ValidationPatterns.VnPhoneMessage)]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email tối đa 255 ký tự")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500, ErrorMessage = "Địa chỉ tối đa 500 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string CustomerAddress { get; set; }
    }

    public class ChangePasswordVM
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mật khẩu mới từ 3 đến 50 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và xác nhận không khớp.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmNewPassword { get; set; }
    }
}
