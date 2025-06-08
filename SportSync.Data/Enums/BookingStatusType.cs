using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum BookingStatusType // Default 1 trong DB design. Cần làm rõ ý nghĩa.
    {
        /// <summary>
        /// Khách hàng đã chọn slot nhưng chưa tải lên bằng chứng thanh toán. 
        /// Slot được giữ tạm thời.
        /// </summary>
        PendingPayment = 0,

        /// <summary>
        /// Khách hàng đã tải lên bằng chứng thanh toán. 
        /// Đơn hàng đang chờ chủ sân duyệt.
        /// Đây là trạng thái quan trọng nhất trong danh sách quản lý của chủ sân.
        /// </summary>
        PendingOwnerConfirmation = 1,

        /// <summary>
        /// Chủ sân đã xác nhận thanh toán và duyệt đơn. 
        /// Slot đã được đặt thành công.
        /// </summary>
        Confirmed = 2,

        /// <summary>
        /// Lịch đặt đã qua và khách hàng đã đến chơi.
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Đơn hàng đã bị hủy bởi chủ sân (ví dụ: thanh toán không hợp lệ, sự cố sân bãi).
        /// </summary>
        CancelledByOwner = 4,

        /// <summary>
        /// Đơn hàng đã bị hủy bởi khách hàng (nếu chính sách cho phép).
        /// </summary>
        CancelledByCustomer = 5,

        /// <summary>
        /// Khách hàng không đến sân theo lịch đã xác nhận.
        /// </summary>
        NoShow = 6
    }
}
