using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Business.Interfaces;
using SportSync.Data;
using SportSync.Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TimeSlotService> _logger;

        public TimeSlotService(ApplicationDbContext context, ILogger<TimeSlotService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TimeSlotOutputDto>> GetTimeSlotsByCourtIdAsync(int courtId)
        {
            var timeSlots = await _context.TimeSlots
                .AsNoTracking()
                .Where(ts => ts.CourtId == courtId)
                .OrderBy(ts => ts.DayOfWeek)
                .ThenBy(ts => ts.StartTime)
                .ToListAsync();

            var cultureInfo = new CultureInfo("vi-VN");
            return timeSlots.Select(ts => new TimeSlotOutputDto
            {
                TimeSlotId = ts.TimeSlotId,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                Price = ts.Price,
                DayOfWeekText = ts.DayOfWeek.HasValue
                    ? cultureInfo.DateTimeFormat.GetDayName((DayOfWeek)ts.DayOfWeek.Value)
                    : "Mọi ngày",
                Notes = ts.Notes,
                IsActiveByOwner = ts.IsActiveByOwner,
                CourtId = ts.CourtId
            });
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> CreateTimeSlotAsync(CreateTimeSlotDto dto, string ownerUserId)
        {
            var errors = new List<string>();

            var court = await _context.Courts
                .Include(c => c.CourtComplex)
                .FirstOrDefaultAsync(c => c.CourtId == dto.CourtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                errors.Add("Sân không tồn tại hoặc bạn không có quyền thêm khung giờ cho sân này.");
                return (false, errors);
            }

            if (dto.EndTime <= dto.StartTime)
            {
                errors.Add("Giờ kết thúc phải sau giờ bắt đầu.");
                return (false, errors);
            }

            // **THAY ĐỔI Ở ĐÂY: Ép kiểu dto.DayOfWeek sang (DayOfWeek?)**
            var existingTimeSlots = await _context.TimeSlots
                .Where(ts => ts.CourtId == dto.CourtId &&
                             (ts.DayOfWeek == (DayOfWeek?)dto.DayOfWeek || ts.DayOfWeek == null || dto.DayOfWeek == null))
                .ToListAsync();

            foreach (var existingSlot in existingTimeSlots)
            {
                if (dto.StartTime < existingSlot.EndTime && dto.EndTime > existingSlot.StartTime)
                {
                    string dayText = existingSlot.DayOfWeek.HasValue ? $"cho {new CultureInfo("vi-VN").DateTimeFormat.GetDayName((DayOfWeek)existingSlot.DayOfWeek.Value)}" : "cho mọi ngày";
                    errors.Add($"Khung giờ mới ({dto.StartTime:HH:mm} - {dto.EndTime:HH:mm}) bị trùng với khung giờ đã có ({existingSlot.StartTime:HH:mm} - {existingSlot.EndTime:HH:mm}) {dayText}.");
                    return (false, errors);
                }
            }

            var newTimeSlot = new TimeSlot
            {
                CourtId = dto.CourtId,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Price = dto.Price,
                DayOfWeek = (DayOfWeek?)dto.DayOfWeek, // Ép kiểu khi gán giá trị
                Notes = dto.Notes,
                IsActiveByOwner = true
            };

            try
            {
                _context.TimeSlots.Add(newTimeSlot);
                await _context.SaveChangesAsync();
                _logger.LogInformation("New timeslot created successfully for CourtId {CourtId}", dto.CourtId);
                return (true, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving new timeslot for CourtId {CourtId}", dto.CourtId);
                errors.Add("Lỗi khi lưu vào cơ sở dữ liệu.");
                return (false, errors);
            }
        }
    }
}
