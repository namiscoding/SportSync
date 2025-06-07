using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace SportSync.Business.Services
{
    public class BookingManagementService : IBookingManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingManagementService> _logger;

        public BookingManagementService(ApplicationDbContext context, ILogger<BookingManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<WeeklyScheduleDto> GetWeeklyScheduleAsync(int courtId, DateTime dateInWeek, string ownerUserId)
        {
            var court = await _context.Courts
                .Include(c => c.CourtComplex)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourtId == courtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                _logger.LogWarning("GetWeeklyScheduleAsync: Court {CourtId} not found or user {OwnerUserId} is not owner.", courtId, ownerUserId);
                return null;
            }

            int diff = (7 + (int)dateInWeek.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            var startOfWeekDateTime = dateInWeek.AddDays(-1 * diff).Date;
            var endOfWeekDateTime = startOfWeekDateTime.AddDays(6).Date;

            var startOfWeek = DateOnly.FromDateTime(startOfWeekDateTime);
            var endOfWeek = DateOnly.FromDateTime(endOfWeekDateTime);

            _logger.LogInformation("GetWeeklyScheduleAsync for CourtId {CourtId}: Fetching data for week {StartOfWeek} to {EndOfWeek}", courtId, startOfWeek.ToString("d"), endOfWeek.ToString("d"));

            var timeSlotTemplates = await _context.TimeSlots
                .AsNoTracking()
                .Where(ts => ts.CourtId == courtId)
                .ToListAsync();

            var actualBookingsInWeek = await _context.BookedSlots
                .AsNoTracking()
                .Include(bs => bs.Booking).ThenInclude(b => b.BookerUser).ThenInclude(u => u.UserProfile)
                .Where(bs => bs.TimeSlot.CourtId == courtId && bs.SlotDate >= startOfWeek && bs.SlotDate <= endOfWeek)
                .ToListAsync();

            var openingTime = court.OpeningTime ?? new TimeOnly(6, 0);
            var closingTime = court.ClosingTime ?? new TimeOnly(23, 0);
            var slotDuration = court.DefaultSlotDurationMinutes > 0 ? court.DefaultSlotDurationMinutes : 60;

            var weeklySchedule = new WeeklyScheduleDto
            {
                CourtId = court.CourtId,
                CourtName = court.Name,
                CourtComplexId = court.CourtComplexId,
                CourtComplexName = court.CourtComplex.Name,
                StartDateOfWeek = startOfWeekDateTime
            };

            for (int i = 0; i < 7; i++)
            {
                var currentDate = startOfWeekDateTime.AddDays(i);
                var dayOfWeek = currentDate.DayOfWeek;
                var currentDateOnly = DateOnly.FromDateTime(currentDate);

                var currentTime = openingTime;
                while (currentTime < closingTime)
                {
                    var nextTime = currentTime.AddMinutes(slotDuration);
                    var newSlot = new ScheduleSlotDto
                    {
                        Date = currentDate,
                        StartTime = currentTime,
                        EndTime = nextTime
                    };

                    var bookedSlot = actualBookingsInWeek.FirstOrDefault(b => b.SlotDate == currentDateOnly && b.ActualStartTime == currentTime);
                    if (bookedSlot != null)
                    {
                        newSlot.Status = "Booked";
                        newSlot.BookingId = bookedSlot.BookingId;
                        newSlot.Price = bookedSlot.PriceAtBookingTime;
                        newSlot.CustomerName = bookedSlot.Booking.ManualBookingCustomerName ?? bookedSlot.Booking.BookerUser?.UserProfile?.FullName ?? bookedSlot.Booking.BookerUser?.UserName ?? "N/A";
                        newSlot.CustomerPhone = bookedSlot.Booking.ManualBookingCustomerPhone ?? bookedSlot.Booking.BookerUser?.PhoneNumber;
                    }
                    else
                    {
                        // **LOGIC TÌM KIẾM TEMPLATE ĐÃ ĐƯỢC CẢI TIẾN**
                        // Bước 1: Ưu tiên tìm template cho ngày cụ thể trong tuần
                        var templateSlot = timeSlotTemplates.FirstOrDefault(t =>
                            t.DayOfWeek == dayOfWeek &&
                            t.StartTime <= currentTime && t.EndTime > currentTime);

                        // Bước 2: Nếu không có, tìm template "Mọi ngày"
                        if (templateSlot == null)
                        {
                            templateSlot = timeSlotTemplates.FirstOrDefault(t =>
                                !t.DayOfWeek.HasValue &&
                                t.StartTime <= currentTime && t.EndTime > currentTime);
                        }

                        if (templateSlot != null)
                        {
                            _logger.LogInformation("Found template for {DayOfWeek} at {StartTime}. IsActive: {IsActive}, Price: {Price}", dayOfWeek, currentTime, templateSlot.IsActiveByOwner, templateSlot.Price);
                            newSlot.Status = templateSlot.IsActiveByOwner ? "Available" : "Closed";
                            newSlot.Price = templateSlot.Price;
                        }
                        else
                        {
                            _logger.LogWarning("No TimeSlot template found for CourtId {CourtId}, Day: {DayOfWeek}, Time: {StartTime}. Defaulting to Closed.", courtId, dayOfWeek, currentTime);
                            newSlot.Status = "Closed"; // Mặc định là đóng nếu không có template
                            newSlot.Price = 0;
                        }
                    }
                    weeklySchedule.Slots.Add(newSlot);
                    currentTime = nextTime;
                }
            }
            return weeklySchedule;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateBulkManualBookingAsync(CreateBulkManualBookingDto dto, string ownerUserId)
        {
            if (dto.Slots == null || !dto.Slots.Any())
            {
                return (false, "Vui lòng chọn ít nhất một khung giờ.");
            }

            var court = await _context.Courts
                .Include(c => c.CourtComplex)
                .FirstOrDefaultAsync(c => c.CourtId == dto.CourtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                return (false, "Sân không tồn tại hoặc bạn không có quyền thực hiện hành động này.");
            }

            // Sử dụng transaction để đảm bảo tất cả các slot được đặt hoặc không có slot nào được đặt
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalPrice = 0;
                var bookedSlotsList = new List<BookedSlot>();
                var slotInfos = dto.Slots.OrderBy(s => s.SlotDate).ThenBy(s => s.StartTime).ToList(); // Sắp xếp để lấy ngày đầu tiên

                foreach (var slotInfo in slotInfos)
                {
                    var slotDateOnly = DateOnly.FromDateTime(slotInfo.SlotDate);

                    // Kiểm tra xem slot này đã bị đặt trong quá trình xử lý chưa
                    var isSlotBooked = await _context.BookedSlots
                        .AnyAsync(bs => bs.TimeSlot.CourtId == dto.CourtId && bs.SlotDate == slotDateOnly && bs.ActualStartTime == slotInfo.StartTime);

                    if (isSlotBooked)
                    {
                        throw new Exception($"Khung giờ {slotInfo.StartTime:HH:mm} ngày {slotDateOnly:dd/MM} đã được người khác đặt trước.");
                    }

                    // Tìm giá và thông tin từ TimeSlot template
                    var templateSlot = await _context.TimeSlots
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(ts => ts.CourtId == dto.CourtId &&
                                                                        (ts.DayOfWeek == slotDateOnly.DayOfWeek || !ts.DayOfWeek.HasValue) &&
                                                                        ts.StartTime <= slotInfo.StartTime && ts.EndTime > slotInfo.StartTime);

                    if (templateSlot == null || !templateSlot.IsActiveByOwner)
                    {
                        throw new Exception($"Khung giờ {slotInfo.StartTime:HH:mm} ngày {slotDateOnly:dd/MM} không hợp lệ hoặc đang đóng.");
                    }

                    totalPrice += templateSlot.Price;
                    bookedSlotsList.Add(new BookedSlot
                    {
                        TimeSlotId = templateSlot.TimeSlotId,
                        SlotDate = slotDateOnly,
                        ActualStartTime = slotInfo.StartTime,
                        ActualEndTime = slotInfo.StartTime.AddMinutes(court.DefaultSlotDurationMinutes),
                        PriceAtBookingTime = templateSlot.Price
                    });
                }

                // Tạo một bản ghi Booking duy nhất cho tất cả các slot
                var newBooking = new Booking
                {
                    BookerUserId = ownerUserId,
                    CourtComplexId = court.CourtComplexId,
                    CourtId = dto.CourtId,
                    BookingDate = DateOnly.FromDateTime(slotInfos.First().SlotDate), // Lấy ngày của slot đầu tiên
                    TotalPrice = totalPrice,
                    BookingStatus = BookingStatusType.Confirmed,
                    PaymentType = PaymentMethodType.PayAtCourt,
                    PaymentStatus = PaymentStatusType.Unpaid,
                    BookingSource = BookingSourceType.ManualByOwner,
                    ManualBookingCustomerName = dto.CustomerName,
                    ManualBookingCustomerPhone = dto.CustomerPhone,
                    CreatedAt = DateTime.UtcNow,
                    BookedSlots = bookedSlotsList // Gán danh sách các BookedSlot
                };

                _context.Bookings.Add(newBooking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk manual booking ({SlotCount} slots) created successfully for CourtId {CourtId} by user {OwnerUserId}", dto.Slots.Count, dto.CourtId, ownerUserId);
                return (true, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating bulk manual booking for CourtId {CourtId}", dto.CourtId);
                return (false, ex.Message);
            }
        }
        public async Task<IEnumerable<BookingListItemDto>> GetBookingsForOwnerAsync(string ownerUserId)
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Court).ThenInclude(c => c.CourtComplex)
                .Include(b => b.BookedSlots)
                .Include(b => b.BookerUser).ThenInclude(u => u.UserProfile) // Include cả UserProfile
                .Where(b => b.Court.CourtComplex.OwnerUserId == ownerUserId)
                .OrderByDescending(b => b.BookingDate)
                .ThenByDescending(b => b.CreatedAt)
                .Select(b => new BookingListItemDto
                {
                    BookingId = b.BookingId,
                    BookerId = b.BookerUserId,
                    // Ưu tiên tên khách vãng lai, sau đó là FullName của user, cuối cùng là UserName
                    CustomerName = b.ManualBookingCustomerName ?? b.BookerUser.UserProfile.FullName ?? b.BookerUser.UserName,
                    CustomerPhone = b.ManualBookingCustomerPhone ?? b.BookerUser.PhoneNumber,
                    CourtName = b.Court.Name,
                    BookingDate = b.BookingDate, // Giờ là DateOnly
                    TimeSlots = string.Join(", ", b.BookedSlots.OrderBy(bs => bs.ActualStartTime).Select(bs => $"{bs.ActualStartTime:HH:mm}-{bs.ActualEndTime:HH:mm}")),
                    TotalPrice = b.TotalPrice,
                    BookingStatus = b.BookingStatus,
                    BookingSource = b.BookingSource,
                    CreatedAt = b.CreatedAt,
                    PaymentProofImageUrl = "https://placehold.co/300x400/667eea/ffffff?text=%E1%BA%A2nh%20chuy%E1%BB%83n%20kho%E1%BA%A3n"//b.PaymentProofImageUrl // Lấy URL ảnh
                })
                .ToListAsync();

            return bookings;
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBookingStatusAsync(long bookingId, BookingStatusType newStatus, string ownerUserId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Court).ThenInclude(c => c.CourtComplex)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null || booking.Court.CourtComplex.OwnerUserId != ownerUserId)
            {
                return (false, "Không tìm thấy đơn đặt hoặc bạn không có quyền thay đổi.");
            }

            // TODO: Thêm logic kiểm tra xem việc chuyển trạng thái có hợp lệ không
            // Ví dụ: chỉ có thể duyệt đơn đang ở trạng thái "Chờ xác nhận"
            // if (booking.BookingStatus != BookingStatusType.PendingConfirmation)
            // {
            //     return (false, "Không thể duyệt một đơn không ở trạng thái 'Chờ xác nhận'.");
            // }

            _logger.LogInformation("Updating booking {BookingId} status from {OldStatus} to {NewStatus} by user {OwnerUserId}",
                bookingId, booking.BookingStatus, newStatus, ownerUserId);

            booking.BookingStatus = newStatus;
            booking.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error updating status for booking {BookingId}", bookingId);
                return (false, "Lỗi khi cập nhật cơ sở dữ liệu.");
            }
        }
        public async Task<BookingBillDto> GetBookingDetailsForBillAsync(long bookingId, string ownerUserId)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Court).ThenInclude(c => c.CourtComplex)
                .Include(b => b.BookedSlots)
                .Include(b => b.BookerUser).ThenInclude(u => u.UserProfile)
                .Where(b => b.BookingId == bookingId && b.Court.CourtComplex.OwnerUserId == ownerUserId)
                .Select(b => new BookingBillDto
                {
                    BookingId = b.BookingId,
                    CustomerName = b.ManualBookingCustomerName ?? b.BookerUser.UserProfile.FullName ?? b.BookerUser.UserName,
                    CustomerPhone = b.ManualBookingCustomerPhone ?? b.BookerUser.PhoneNumber,
                    CourtName = b.Court.Name,
                    CourtComplexName = b.Court.CourtComplex.Name,
                    CourtComplexAddress = b.Court.CourtComplex.Address,
                    BookingDate = b.BookingDate,
                    TotalPrice = b.TotalPrice,
                    CreatedAt = b.CreatedAt,
                    BookedSlots = b.BookedSlots.Select(bs => new BookedSlotInfo
                    {
                        StartTime = bs.ActualStartTime,
                        EndTime = bs.ActualEndTime,
                        Price = bs.PriceAtBookingTime
                    }).OrderBy(s => s.StartTime).ToList()
                })
                .FirstOrDefaultAsync();

            return booking;
        }
    }
}
