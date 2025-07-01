// File: Controllers/AdminDashboardController.cs
using Microsoft.AspNetCore.Mvc;
using SportSync.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SportSync.Data.Entities;

namespace SportSync.Web.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? selectedDate)
        {
            // Tổng số người dùng
            var totalUsers = await _context.Users.CountAsync();

            // Tổng số customer
            var totalCustomers = (await _userManager.GetUsersInRoleAsync("Customer")).Count;
            // Tổng số owner (bao gồm cả CourtOwner, StandardCourtOwner, ProCourtOwner nếu có)
            var totalOwners = (await _userManager.GetUsersInRoleAsync("CourtOwner")).Count;
            // Nếu có các role owner khác, cộng thêm:
            //if (await _userManager.RoleExistsAsync("StandardCourtOwner"))
            //    totalOwners += (await _userManager.GetUsersInRoleAsync("StandardCourtOwner")).Count;
            //if (await _userManager.RoleExistsAsync("ProCourtOwner"))
            //    totalOwners += (await _userManager.GetUsersInRoleAsync("ProCourtOwner")).Count;

            // Tổng số người dùng đã từng đặt sân
            var totalBookers = await _context.Bookings.Select(b => b.BookerUserId).Distinct().CountAsync();

            // Tỉ lệ chuyển đổi người dùng -> người đặt sân
            double conversionRate = totalUsers > 0 ? (double)totalBookers / totalUsers * 100 : 0;

            // Xác định ngày mốc
            DateTime baseDate;
            if (!string.IsNullOrEmpty(selectedDate) && DateTime.TryParse(selectedDate, out var parsedDate))
                baseDate = parsedDate.Date;
            else
                baseDate = await _context.Bookings.MaxAsync(b => (DateTime?)b.BookedStartTime.Date) ?? DateTime.Today;

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => baseDate.AddDays(-6 + i))
                .ToList();

            var bookingCounts = await _context.Bookings
                .Where(b => b.BookedStartTime.Date >= last7Days.First() && b.BookedStartTime.Date <= last7Days.Last())
                .GroupBy(b => b.BookedStartTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentBookings = last7Days
                .Select(date => new {
                    Date = date,
                    Count = bookingCounts.FirstOrDefault(x => x.Date == date)?.Count ?? 0
                })
                .ToList();
// Trung bình thời gian phản hồi đặt sân (từ tạo đến cập nhật)
            var responseTimes = await _context.Bookings
                .Where(b => b.UpdatedAt != null)
                .Select(b => EF.Functions.DateDiffSecond(b.CreatedAt, b.UpdatedAt.Value))
                .ToListAsync();

            double avgResponseTimeSeconds = responseTimes.Count > 0 ? responseTimes.Average() : 0;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCustomers = totalCustomers;
            ViewBag.TotalOwners = totalOwners;
            ViewBag.TotalBookers = totalBookers;
            ViewBag.ConversionRate = conversionRate;
            ViewBag.RecentBookings = recentBookings;
            ViewBag.AvgResponseTimeSeconds = avgResponseTimeSeconds;
            ViewBag.SelectedDate = baseDate.ToString("yyyy-MM-dd");

            return View();
        }
    }
}
