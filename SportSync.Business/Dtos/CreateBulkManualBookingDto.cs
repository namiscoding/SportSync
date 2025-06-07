using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class CreateBulkManualBookingDto
    {
        [Required]
        public int CourtId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Vui lòng chọn ít nhất một khung giờ.")]
        public List<BookingSlotInfo> Slots { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khách hàng.")]
        [StringLength(255)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại khách hàng.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string CustomerPhone { get; set; }
    }
}
