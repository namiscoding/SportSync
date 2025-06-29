// File: Controllers/AdminDashboardController.cs
using Microsoft.AspNetCore.Mvc;
using SportSync.Data;
using Microsoft.EntityFrameworkCore;

namespace SportSync.Web.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Tổng số người dùng
            var totalUsers = await _context.Users.CountAsync();

            // Tổng số người dùng đã từng đặt sân
            var totalBookers = await _context.Bookings.Select(b => b.BookerUserId).Distinct().CountAsync();

            // Tỉ lệ chuyển đổi người dùng -> người đặt sân
            double conversionRate = totalUsers > 0 ? (double)totalBookers / totalUsers * 100 : 0;

            // Tổng số lượt đặt sân trong 7 ngày gần nhất
            var past7Days = DateTime.Now.AddDays(-7);
            var recentBookings = await _context.Bookings
                .Where(b => b.CreatedAt >= past7Days)
                .GroupBy(b => b.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            // Trung bình thời gian phản hồi đặt sân (từ tạo đến cập nhật)
            var responseTimes = await _context.Bookings
                .Where(b => b.UpdatedAt != null)
                .Select(b => EF.Functions.DateDiffSecond(b.CreatedAt, b.UpdatedAt.Value))
                .ToListAsync();

            double avgResponseTimeSeconds = responseTimes.Count > 0 ? responseTimes.Average() : 0;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalBookers = totalBookers;
            ViewBag.ConversionRate = conversionRate;
            ViewBag.RecentBookings = recentBookings;
            ViewBag.AvgResponseTimeSeconds = avgResponseTimeSeconds;

            return View();
        }
    }
}
