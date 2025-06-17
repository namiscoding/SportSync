using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Dtos.OwnerDashboard;
using SportSync.Business.Interfaces;
using SportSync.Data;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class CourtOwnerDashboardService : ICourtOwnerDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourtOwnerDashboardService> _logger;

        public CourtOwnerDashboardService(ApplicationDbContext context, ILogger<CourtOwnerDashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<CourtOwnerDashboardDto> GetDashboardDataAsync(string ownerUserId)
        {
            throw new NotImplementedException();
        }

        //public async Task<CourtOwnerDashboardDto> GetDashboardDataAsync(string ownerUserId)
        //{
        //    var dashboardData = new CourtOwnerDashboardDto();

        //    var courtComplex = await _context.CourtComplexes
        //        .Include(cc => cc.Courts)
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(cc => cc.OwnerUserId == ownerUserId);

        //    if (courtComplex == null)
        //    {
        //        dashboardData.HasComplex = false;
        //        return dashboardData;
        //    }

        //    dashboardData.HasComplex = true;

        //    // A. Lấy thông tin phức hợp sân
        //    dashboardData.ComplexInfo = new ComplexInfoSectionDto
        //    {
        //        Id = courtComplex.CourtComplexId,
        //        Name = courtComplex.Name,
        //        Address = $"{courtComplex.Address}, {courtComplex.Ward}, {courtComplex.District}, {courtComplex.City}",
        //        ImageUrl = courtComplex.MainImageCloudinaryUrl,
        //        CourtCount = courtComplex.Courts.Count,
        //        IsActive = courtComplex.IsActiveByOwner
        //    };

        //    DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        //    var courtIds = courtComplex.Courts.Select(c => c.CourtId).ToList();

        //    var allBookings = await _context.Bookings
        //        .Include(b => b.BookerUser).ThenInclude(u => u.UserProfile)
        //        .Include(b => b.Court)
        //        .Include(b => b.BookedSlots)
        //        .Where(b => courtIds.Contains(b.CourtId))
        //        .AsNoTracking()
        //        .ToListAsync();

        //    // B. Lấy số liệu thống kê
        //    var todayConfirmedBookings = allBookings
        //        .Where(b => b.BookingDate == today && (b.BookingStatus == BookingStatusType.Confirmed || b.BookingStatus == BookingStatusType.Completed))
        //        .ToList();

        //    dashboardData.Statistics = new StatsSectionDto
        //    {
        //        TodayBookingCount = todayConfirmedBookings.Count,
        //        TodayRevenue = todayConfirmedBookings.Sum(b => b.TotalPrice),
        //        MaintenanceCourtCount = courtComplex.Courts.Count(c => c.StatusByOwner == CourtStatusByOwner.Suspended),
        //        OccupancyRate = 0 // Tạm thời
        //    };

        //    // C. Lấy lịch đặt sân hôm nay (bao gồm cả đơn chờ duyệt)
        //    var todayAllStatusBookings = allBookings.Where(b => b.BookingDate == today).ToList();

        //    dashboardData.TodaySchedule = todayAllStatusBookings
        //        .Select(b =>
        //        {
        //            var sortedSlots = b.BookedSlots.OrderBy(s => s.ActualStartTime).ToList();
        //            var startTime = sortedSlots.FirstOrDefault()?.ActualStartTime ?? new TimeOnly(0, 0);
        //            var endTime = sortedSlots.LastOrDefault()?.ActualEndTime ?? new TimeOnly(0, 0);

        //            return new TodaysBookingDto
        //            {
        //                BookingId = b.BookingId,
        //                CourtName = b.Court.Name,
        //                TimeRange = $"{startTime:HH:mm} - {endTime:HH:mm}",
        //                CustomerName = b.ManualBookingCustomerName ?? b.BookerUser.UserProfile.FullName ?? b.BookerUser.UserName,
        //                CustomerPhone = b.ManualBookingCustomerPhone ?? b.BookerUser.PhoneNumber,
        //                TotalPrice = b.TotalPrice,
        //                Status = b.BookingStatus, // **GÁN TRẠNG THÁI**
        //                PaymentProofImageUrl = "https://placehold.co/300x400/667eea/ffffff?text=%E1%BA%A2nh%20chuy%E1%BB%83n%20kho%E1%BA%A3n"//b.PaymentProofImageUrl // Lấy URL ảnh

        //            };
        //        })
        //        .OrderBy(b => b.TimeRange).ThenBy(b => b.CourtName)
        //        .ToList();

        //    // D. Lấy các đơn đang chờ duyệt (giữ nguyên)
        //    dashboardData.PendingBookings = allBookings
        //        .Where(b => b.BookingStatus == BookingStatusType.PendingOwnerConfirmation)
        //        .OrderBy(b => b.CreatedAt)
        //        .Select(b => new PendingBookingDto
        //        {
        //            BookingId = b.BookingId,
        //            CustomerName = b.ManualBookingCustomerName ?? b.BookerUser.UserProfile.FullName ?? b.BookerUser.UserName,
        //            CreatedAt = b.CreatedAt
        //        })
        //        .Take(5)
        //        .ToList();

        //    return dashboardData;
        //}
    }
}
