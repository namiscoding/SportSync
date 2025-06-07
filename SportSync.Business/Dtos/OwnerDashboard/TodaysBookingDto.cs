using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Business.Dtos.OwnerDashboard
{
    public class TodaysBookingDto
    {
        public long BookingId { get; set; }
        public string CourtName { get; set; }
        public string TimeRange { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; } // Thêm SĐT để hiển thị trong modal
        public decimal TotalPrice { get; set; }
        public BookingStatusType Status { get; set; } // **THÊM TRẠNG THÁI**
        public string? PaymentProofImageUrl { get; set; } // **THÊM ẢNH THANH TOÁN**
    }
}
