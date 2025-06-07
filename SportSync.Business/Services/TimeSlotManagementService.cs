using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using SportSync.Data;

namespace SportSync.Business.Services
{
    public class TimeSlotManagementService : ITimeSlotManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimeSlotManagementService> _logger;

        public TimeSlotManagementService(ApplicationDbContext context, ILogger<TimeSlotManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<TimeSlotTemplateDto> GetTimeSlotTemplateForCourtAsync(int courtId, string ownerUserId)
        {
            var court = await _context.Courts
                .Include(c => c.CourtComplex)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CourtId == courtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                _logger.LogWarning("GetTimeSlotTemplateForCourtAsync: Court {CourtId} not found or user {OwnerUserId} is not owner.", courtId, ownerUserId);
                return null; // Controller sẽ xử lý lỗi not found hoặc forbid
            }

            var openingTime = court.OpeningTime ?? new TimeOnly(6, 0); // Giờ mặc định nếu null
            var closingTime = court.ClosingTime ?? new TimeOnly(23, 0);
            var slotDuration = court.DefaultSlotDurationMinutes;
            if (slotDuration <= 0) slotDuration = 60; // Mặc định 60 phút nếu chưa set

            // Lấy tất cả các TimeSlot đã được định nghĩa trước cho sân này
            var existingSlots = await _context.TimeSlots
                .AsNoTracking()
                .Where(ts => ts.CourtId == courtId)
                .ToListAsync();

            var template = new TimeSlotTemplateDto
            {
                CourtId = court.CourtId,
                CourtName = court.Name,
                CourtComplexId = court.CourtComplexId,
                CourtComplexName = court.CourtComplex.Name,
                OpeningTime = openingTime,
                ClosingTime = closingTime,
                SlotDurationMinutes = slotDuration
            };

            // Tạo lưới lịch đầy đủ và điền thông tin từ các slot đã có
            var currentTime = openingTime;
            while (currentTime < closingTime)
            {
                var nextTime = currentTime.AddMinutes(slotDuration);
                for (int day = 0; day <= 6; day++) // 0=Sunday, ..., 6=Saturday
                {
                    // Tìm slot tương ứng trong DB
                    var dbSlot = existingSlots.FirstOrDefault(s => s.DayOfWeek == (DayOfWeek)day && s.StartTime == currentTime);

                    template.TimeSlots.Add(new TimeSlotInfo
                    {
                        SlotId = $"slot-{day}-{currentTime:HH:mm}",
                        DayOfWeek = day,
                        StartTime = currentTime,
                        Price = dbSlot?.Price ?? 200000, // Giá mặc định nếu chưa có
                        IsClosed = dbSlot?.IsActiveByOwner == false
                    });
                }
                currentTime = nextTime;
            }

            return template;
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateBulkTimeSlotsAsync(BulkTimeSlotUpdateDto updateData, string ownerUserId)
        {
            var court = await _context.Courts
                .Include(c => c.CourtComplex)
                .FirstOrDefaultAsync(c => c.CourtId == updateData.CourtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                return (false, "Sân không tồn tại hoặc bạn không có quyền chỉnh sửa.");
            }

            // Lấy tất cả TimeSlot template hiện có của sân này để so sánh và cập nhật
            var existingTemplates = await _context.TimeSlots
                .Where(ts => ts.CourtId == updateData.CourtId)
                .ToListAsync();

            try
            {
                // Chỉ lặp qua những thay đổi được gửi từ client
                foreach (var change in updateData.Changes)
                {
                    var slotInfo = change.Value;

                    // Tìm template đã có trong DB khớp với thông tin thay đổi từ client
                    var dbTemplate = existingTemplates.FirstOrDefault(t =>
                        t.DayOfWeek == (DayOfWeek)slotInfo.DayOfWeek &&
                        t.StartTime == slotInfo.StartTime);

                    if (dbTemplate != null)
                    {
                        // **UPDATE EXISTING TEMPLATE**
                        _logger.LogInformation("Updating TimeSlotId {TimeSlotId} with Price: {Price}, Status: {Status}", dbTemplate.TimeSlotId, slotInfo.NewPrice, slotInfo.NewStatus);

                        if (slotInfo.NewPrice.HasValue)
                        {
                            dbTemplate.Price = slotInfo.NewPrice.Value;
                        }
                        if (!string.IsNullOrEmpty(slotInfo.NewStatus))
                        {
                            dbTemplate.IsActiveByOwner = slotInfo.NewStatus == "Available";
                        }
                        _context.TimeSlots.Update(dbTemplate);
                    }
                    else
                    {
                        // **CREATE NEW TEMPLATE**
                        _logger.LogInformation("Creating new TimeSlot for CourtId {CourtId} at {DayOfWeek} {StartTime}", court.CourtId, (DayOfWeek)slotInfo.DayOfWeek, slotInfo.StartTime);
                        var newTemplate = new TimeSlot
                        {
                            CourtId = updateData.CourtId,
                            DayOfWeek = (DayOfWeek)slotInfo.DayOfWeek,
                            StartTime = slotInfo.StartTime,
                            EndTime = slotInfo.StartTime.AddMinutes(court.DefaultSlotDurationMinutes),
                            Price = slotInfo.NewPrice ?? 200000, // Giá mặc định nếu không có
                            IsActiveByOwner = !string.IsNullOrEmpty(slotInfo.NewStatus) ? (slotInfo.NewStatus == "Available") : true // Mặc định là Available nếu không có status
                        };
                        _context.TimeSlots.Add(newTemplate);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk timeslot templates updated successfully for CourtId {CourtId}", updateData.CourtId);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk timeslot template update for CourtId {CourtId}", updateData.CourtId);
                return (false, "Đã xảy ra lỗi hệ thống khi cập nhật lịch trình.");
            }
        }
    }
}
