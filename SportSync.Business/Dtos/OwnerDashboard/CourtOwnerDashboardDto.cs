using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos.OwnerDashboard
{
    public class CourtOwnerDashboardDto
    {
        public bool HasComplex { get; set; } // Kiểm tra xem chủ sân đã tạo khu phức hợp chưa
        public ComplexInfoSectionDto ComplexInfo { get; set; }
        public StatsSectionDto Statistics { get; set; }
        public List<TodaysBookingDto> TodaySchedule { get; set; }
        public List<PendingBookingDto> PendingBookings { get; set; }
    }
}
