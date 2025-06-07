using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos.OwnerDashboard
{
    public class StatsSectionDto
    {
        public int TodayBookingCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public double OccupancyRate { get; set; } // Tỷ lệ lấp đầy (0-100)
        public int MaintenanceCourtCount { get; set; }
    }
}
