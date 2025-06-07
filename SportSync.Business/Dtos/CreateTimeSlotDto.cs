using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class CreateTimeSlotDto
    {
        [Required]
        public int CourtId { get; set; } // Sân mà khung giờ này thuộc về

        [Required(ErrorMessage = "Vui lòng nhập giờ bắt đầu.")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ bắt đầu")]
        public TimeOnly StartTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giờ kết thúc.")]
        [DataType(DataType.Time)]
        [Display(Name = "Giờ kết thúc")]
        public TimeOnly EndTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá tiền.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tiền không được là số âm.")]
        [Display(Name = "Giá (VNĐ)")]
        public decimal Price { get; set; }

        // DayOfWeek là nullable int. Null nghĩa là áp dụng cho mọi ngày.
        // Giá trị từ 0 (Chủ Nhật) đến 6 (Thứ Bảy).
        [Display(Name = "Áp dụng cho ngày")]
        public int? DayOfWeek { get; set; }

        [StringLength(255)]
        [Display(Name = "Ghi chú (Tùy chọn)")]
        public string? Notes { get; set; }
    }
}
