using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum BookingStatusType // Default 1 trong DB design. Cần làm rõ ý nghĩa.
    {
        // --- Các trạng thái chính ---
        Confirmed = 1,              // Đã xác nhận (Tự động khi đặt thành công cho MVP "Thanh toán tại sân",
                                    // hoặc sau khi thanh toán online thành công trong tương lai)

        Completed = 2,              // Đã hoàn thành (Người đặt đã sử dụng sân)
        CancelledByBooker = 3,      // Người đặt đã hủy
        CancelledByOwner = 4,       // Chủ sân đã hủy (ví dụ: sự cố đột xuất với sân)
        NoShow = 5,                  // Khách không đến (tùy chọn)
    }
}
