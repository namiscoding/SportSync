using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public class BookingBillDto
    {
        public long BookingId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CourtName { get; set; }
        public string CourtComplexName { get; set; }
        public string CourtComplexAddress { get; set; }
        public DateOnly BookingDate { get; set; }
        public List<BookedSlotInfo> BookedSlots { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } // Ngày tạo đơn
    }
}
