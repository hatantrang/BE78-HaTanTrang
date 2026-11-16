using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using _25DH110581_Hà_Tấn_Trạng_MyStore.Models;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models.ViewModel
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [RegularExpression(ValidationPatterns.Username, ErrorMessage = ValidationPatterns.UsernameMessage)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mật khẩu từ 3 đến 50 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 255 ký tự")]
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
}
