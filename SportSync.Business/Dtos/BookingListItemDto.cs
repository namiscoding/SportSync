using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Business.Dtos
{
    public class BookingListItemDto
    {
        public long BookingId { get; set; }

        // Phân biệt khách vãng lai và người dùng đã đăng ký
        public string BookerId { get; set; } // UserId của người đặt nếu là user đã đăng ký
        public string CustomerName { get; set; } // Tên khách vãng lai hoặc FullName của user
        public string CustomerPhone { get; set; }

        public string CourtName { get; set; }
        public DateOnly BookingDate { get; set; } // **SỬA THÀNH DateOnly ĐỂ KHỚP VỚI DB**
        public string TimeSlots { get; set; } // Chuỗi hiển thị các khung giờ
        public decimal TotalPrice { get; set; }
        public BookingStatusType BookingStatus { get; set; }
        public BookingSourceType BookingSource { get; set; }

        // **CÁC TRƯỜNG MỚI**
        public DateTime CreatedAt { get; set; } // Ngày tạo đơn đặt
        public string? PaymentProofImageUrl { get; set; } // URL ảnh bằng chứng thanh toán
    }
}
