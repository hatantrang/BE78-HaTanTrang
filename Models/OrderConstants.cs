using System.Collections.Generic;

namespace _25DH110581_Hà_Tấn_Trạng_MyStore.Models
{
    public static class OrderStatuses
    {
        public const string AwaitingPayment = "Chờ thanh toán";
        public const string Processing = "Đang xử lý";
        public const string Shipping = "Đang vận chuyển";
        public const string Delivered = "Đã giao";
        public const string Cancelled = "Đã hủy";

        public static readonly string[] All =
        {
            AwaitingPayment, Processing, Shipping, Delivered, Cancelled
        };
    }

    public static class PaymentMethods
    {
        public const string Cash = "Tiền mặt";
   
        public const string PayLater = "Mua trước trả sau";

        public const string Momo = "Ví Momo";

        public const string VNPAY = "Ví VNPAY";
        public static readonly string[] All = { Cash, PayLater, Momo, VNPAY };
    }

    public static class DeliveryMethods
    {
        public const string Standard = "Giao hàng tiêu chuẩn";
        public const string Express = "Giao hàng nhanh";

        public static readonly string[] All = { Standard, Express };
    }

    public static class PaymentStatuses
    {
        public const string Unpaid = "Chưa thanh toán";
        public const string Pending = "Chờ thanh toán";
        public const string Paid = "Đã thanh toán";
    }
}
