using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportSync.Business.Interfaces;
using SportSync.Business.Dtos;
using SportSync.Data;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class CourtComplexService : ICourtComplexService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<CourtComplexService> _logger;

        public CourtComplexService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IImageUploadService imageUploadService,
            ILogger<CourtComplexService> logger)
        {
            _context = context;
            _userManager = userManager;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        public async Task<IEnumerable<CourtComplex>> GetCourtComplexesByOwnerAsync(string ownerUserId)
        {
            if (string.IsNullOrEmpty(ownerUserId))
            {
                _logger.LogWarning("GetCourtComplexesByOwnerAsync: ownerUserId is null or empty.");
                return new List<CourtComplex>();
            }

            try
            {
                return await _context.CourtComplexes
                                     .Where(cc => cc.OwnerUserId == ownerUserId)
                                     .OrderByDescending(cc => cc.CreatedAt)
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching court complexes for owner {OwnerUserId}", ownerUserId);
                return new List<CourtComplex>();
            }
        }

        public async Task<(bool Success, CourtComplex CreatedComplex, IEnumerable<string> Errors)> CreateCourtComplexAsync(CreateCourtComplexDto dto, string ownerUserId)
        {
            var errors = new List<string>();
            if (dto == null)
            {
                errors.Add("Dữ liệu khu phức hợp không được để trống.");
                return (false, null, errors);
            }
            if (string.IsNullOrEmpty(ownerUserId))
            {
                errors.Add("Không xác định được người sở hữu.");
                return (false, null, errors);
            }

            // **KIỂM TRA NẾU CHỦ SÂN ĐÃ CÓ KHU PHỨC HỢP CHƯA**
            var existingComplexesCount = await _context.CourtComplexes
                                                 .CountAsync(cc => cc.OwnerUserId == ownerUserId);

            if (existingComplexesCount > 0)
            {
                _logger.LogWarning("User {OwnerUserId} attempted to create a new court complex but already owns {Count} complex(es).", ownerUserId, existingComplexesCount);
                errors.Add("Mỗi chủ sân chỉ được quản lý một khu phức hợp sân. Bạn đã có một khu phức hợp.");
                return (false, null, errors);
            }

            var courtComplex = new CourtComplex
            {
                OwnerUserId = ownerUserId,
                Name = dto.Name,
                Address = dto.Address,
                City = dto.City, // Giả sử đây là code, cần xử lý lấy tên nếu muốn lưu tên
                District = dto.District, // Giả sử đây là code
                Ward = dto.Ward, // Giả sử đây là code
                Description = dto.Description,
                ContactPhoneNumber = dto.ContactPhoneNumber,
                ContactEmail = dto.ContactEmail,
                DefaultOpeningTime = dto.DefaultOpeningTime,
                DefaultClosingTime = dto.DefaultClosingTime,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CreatedAt = DateTime.UtcNow,
                ApprovalStatus = ApprovalStatus.PendingApproval,
                IsActiveByOwner = true,
                IsActiveByAdmin = false
            };

            if (dto.MainImage != null && dto.MainImage.Content != null && dto.MainImage.Content.Length > 0)
            {
                _logger.LogInformation("Attempting to upload main image for new court complex: {FileName}", dto.MainImage.FileName);
                string folderName = $"court_complexes/{Guid.NewGuid()}";

                var uploadResult = await _imageUploadService.UploadImageAsync(dto.MainImage, folderName);

                if (uploadResult.Success)
                {
                    courtComplex.MainImageCloudinaryPublicId = uploadResult.PublicId;
                    courtComplex.MainImageCloudinaryUrl = uploadResult.Url;
                    _logger.LogInformation("Main image uploaded successfully: {ImageUrl}", uploadResult.Url);
                }
                else
                {
                    _logger.LogWarning("Main image upload failed: {ErrorMessage}", uploadResult.ErrorMessage);
                    errors.Add("Lỗi tải ảnh đại diện: " + (uploadResult.ErrorMessage ?? "Không xác định."));
                }
            }

            if (errors.Any(e => e.StartsWith("Lỗi tải ảnh đại diện")))
            {
                // return (false, null, errors); // Quyết định có dừng nếu lỗi ảnh không
            }

            try
            {
                _context.CourtComplexes.Add(courtComplex);
                await _context.SaveChangesAsync();
                _logger.LogInformation("CourtComplex '{ComplexName}' created successfully by User {OwnerUserId}. ID: {ComplexId}", courtComplex.Name, ownerUserId, courtComplex.CourtComplexId);
                return (true, courtComplex, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while creating CourtComplex '{ComplexName}' by User {OwnerUserId}.", dto.Name, ownerUserId);
                errors.Add("Đã xảy ra lỗi khi lưu thông tin vào cơ sở dữ liệu. Vui lòng thử lại.");
                return (false, null, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic exception while creating CourtComplex '{ComplexName}' by User {OwnerUserId}.", dto.Name, ownerUserId);
                errors.Add("Đã xảy ra lỗi không mong muốn. Vui lòng thử lại.");
                return (false, null, errors);
            }
        }

        public async Task<CourtComplexDetailDto?> GetDetailAsync(int complexId, DateOnly? date, CancellationToken ct = default)
        {
            int? dow = date.HasValue ? (int)date.Value.DayOfWeek : null;

            return await _context.CourtComplexes
                .AsNoTracking()
                .Where(c => c.CourtComplexId == complexId &&
                            c.ApprovalStatus == ApprovalStatus.Approved &&
                            c.IsActiveByOwner && c.IsActiveByAdmin)
                .Select(c => new CourtComplexDetailDto
                {
                    ComplexId = c.CourtComplexId,
                    Name = c.Name,
                    Address = c.Address,
                    Description = c.Description,
                    ThumbnailUrl = c.MainImageCloudinaryUrl,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude,
                    ContactPhone = c.ContactPhoneNumber,
                    ContactEmail = c.ContactEmail,

                    Courts = c.Courts
                        .Where(co => co.IsActiveByAdmin &&
                                     co.StatusByOwner == CourtStatusByOwner.Available)
                        .Select(co => new CourtDetailDto
                        {
                            CourtId = co.CourtId,
                            Name = co.Name,
                            SportTypeName = co.SportType.Name,
                            ImageUrl = co.MainImageCloudinaryUrl,

                            Amenities = co.CourtAmenities
                                          .Select(a => new AmenityDto(a.Amenity.Name)),

                            AvailableSlots = co.TimeSlots
                                .Where(ts => ts.IsActiveByOwner &&
                                             // lọc theo thứ
                                             (!dow.HasValue || ts.DayOfWeek == null || (int)ts.DayOfWeek == dow) &&
                                             // loại slot đã được đặt
                                             (!date.HasValue || !_context.BookedSlots.Any(bs =>
                                                  bs.TimeSlotId == ts.TimeSlotId &&
                                                  bs.SlotDate == date &&
                                                  bs.Booking.BookingStatus == BookingStatusType.Confirmed)))
                                .Select(ts => new TimeSlotDto
                                {
                                    TimeSlotId = ts.TimeSlotId,
                                    Start = ts.StartTime,
                                    End = ts.EndTime,
                                    Price = ts.Price
                                })
                        })
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
