using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Enums
{
    public enum NotificationContentType
    {
        // Liên quan đến Đặt sân
        BookingConfirmedToUser = 1,
        BookingConfirmedToOwner = 2,
        BookingCancelledByBookerToUser = 3,
        BookingCancelledByBookerToOwner = 4,
        BookingCancelledByOwnerToUser = 5,
        BookingReminderToUser = 6,      // Nhắc lịch sắp tới
        BookingCompleted = 7,           // Thông báo hoàn thành (ví dụ: mời đánh giá)
        BookingNoShow = 8,              // Thông báo khách không đến (cho chủ sân)

        // Liên quan đến Tài khoản
        AccountRegistered = 20,
        AccountApprovedByAdmin = 21,    // Tài khoản chủ sân được duyệt
        AccountRejectedByAdmin = 22,    // Tài khoản chủ sân bị từ chối
        AccountSuspended = 23,
        PasswordChanged = 24,
        PasswordResetRequest = 25,

        // Liên quan đến Hệ thống sân (Court Complex)
        CourtComplexApproved = 30,
        CourtComplexRejected = 31,
        CourtComplexDeactivatedByAdmin = 32,

        // Liên quan đến Hệ thống chung
        SystemMaintenance = 50,
        NewFeatureUpdate = 51,
        PolicyUpdate = 52,
        Promotion = 60                  // Thông báo khuyến mãi
    }
}
