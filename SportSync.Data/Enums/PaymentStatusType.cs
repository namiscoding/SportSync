using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum PaymentStatusType // Default 0 trong DB design
    {
        Unpaid = 0,         // Chưa thanh toán
        Paid = 1,           // Đã thanh toán
        Refunded = 2,       // Đã hoàn tiền
        PartiallyPaid = 3,  // Thanh toán một phần (tùy chọn)
        PaymentFailed = 4   // Thanh toán thất bại (tùy chọn)
    }
}
