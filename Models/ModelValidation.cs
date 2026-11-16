using System.ComponentModel.DataAnnotations;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models
{
    public static class ValidationPatterns
    {
        public const string VnPhone = @"^(0|\+84)(3|5|7|8|9)[0-9]{8}$";
        public const string VnPhoneMessage = "Số điện thoại không hợp lệ (10 số bắt đầu bằng 0 hoặc +84, ví dụ 0901234567)";

        public const string Username = @"^[A-Za-z0-9_.\-]{3,50}$";
        public const string UsernameMessage = "Tên đăng nhập 3-50 ký tự, chỉ gồm chữ, số, dấu chấm, gạch dưới hoặc gạch ngang";
    }

    [MetadataType(typeof(CategoryMetadata))]
    public partial class Category
    {
    }

    public class CategoryMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 255 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }
    }

    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
    }

    public class ProductMetadata
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(255, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ 2 đến 255 ký tự")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả sản phẩm")]
        [StringLength(4000, ErrorMessage = "Mô tả tối đa 4000 ký tự")]
        public string ProductDescription { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Giá phải từ 0 đến 1.000.000.000 đ")]
        public decimal ProductPrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Số lượng tồn phải là số nguyên từ 0 đến 1.000.000")]
        public int ProductStock { get; set; }
    }
}
