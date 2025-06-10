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
                City = dto.City,
                District = dto.District,
                Ward = dto.Ward,
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

            // **THAY ĐỔI Ở ĐÂY: Xử lý IFormFile**
            if (dto.MainImageFile != null && dto.MainImageFile.Length > 0)
            {
                _logger.LogInformation("Attempting to upload main image for new court complex: {FileName}, Size: {FileSize}", dto.MainImageFile.FileName, dto.MainImageFile.Length);
                string folderName = $"court_complexes/{Guid.NewGuid()}";

                ImageInputDto imageInputForService = null;
                try
                {
                    // Tạo MemoryStream để copy dữ liệu từ IFormFile
                    // Stream này sẽ được CloudinaryImageUploadService dispose
                    var memoryStream = new MemoryStream();
                    await dto.MainImageFile.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Reset vị trí stream

                    imageInputForService = new ImageInputDto
                    {
                        Content = memoryStream, // Truyền MemoryStream
                        FileName = dto.MainImageFile.FileName,
                        ContentType = dto.MainImageFile.ContentType
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error preparing image stream from IFormFile for {FileName}", dto.MainImageFile.FileName);
                    errors.Add("Lỗi xử lý file ảnh: " + ex.Message);
                    // Không return ngay, để vẫn tạo complex nếu người dùng chấp nhận không có ảnh
                }


                if (imageInputForService != null && imageInputForService.Content != null)
                {
                    var uploadResult = await _imageUploadService.UploadImageAsync(imageInputForService, folderName);

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
                        // Không return ngay, vẫn cho tạo complex không ảnh nếu muốn
                    }
                }
            }

            // Nếu có lỗi nghiêm trọng (ví dụ lỗi xử lý file ảnh mà bạn không muốn tiếp tục), bạn có thể kiểm tra `errors.Any()` ở đây.
            // Hiện tại, chúng ta sẽ tiếp tục tạo complex ngay cả khi ảnh lỗi.

            try
            {
                _context.CourtComplexes.Add(courtComplex);
                await _context.SaveChangesAsync();
                _logger.LogInformation("CourtComplex '{ComplexName}' created successfully by User {OwnerUserId}. ID: {ComplexId}", courtComplex.Name, ownerUserId, courtComplex.CourtComplexId);
                // Nếu có lỗi tải ảnh trước đó nhưng vẫn tạo complex, chúng ta không trả về errors đó như một lỗi của việc tạo complex
                // Trừ khi bạn muốn thông báo cả lỗi ảnh cho người dùng.
                // Hiện tại, nếu tạo complex thành công thì trả về success = true.
                return (true, courtComplex, errors.Any() ? errors : null); // Trả về errors nếu có (ví dụ lỗi ảnh)
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

        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateCourtComplexAsync(UpdateCourtComplexDto dto, string ownerUserId)
        {
            var errors = new List<string>();
            var complexToUpdate = await _context.CourtComplexes.FindAsync(dto.Id);

            if (complexToUpdate == null || complexToUpdate.OwnerUserId != ownerUserId)
            {
                _logger.LogWarning("Update attempt failed. Complex {ComplexId} not found or user {OwnerUserId} is not the owner.", dto.Id, ownerUserId);
                errors.Add("Không tìm thấy khu phức hợp hoặc bạn không có quyền chỉnh sửa.");
                return (false, errors);
            }

            // Cập nhật các thuộc tính
            complexToUpdate.Name = dto.Name;
            complexToUpdate.Address = dto.Address;
            complexToUpdate.City = dto.City;
            complexToUpdate.District = dto.District;
            complexToUpdate.Ward = dto.Ward;
            complexToUpdate.Description = dto.Description;
            complexToUpdate.ContactPhoneNumber = dto.ContactPhoneNumber;
            complexToUpdate.ContactEmail = dto.ContactEmail;
            complexToUpdate.DefaultOpeningTime = dto.DefaultOpeningTime;
            complexToUpdate.DefaultClosingTime = dto.DefaultClosingTime;
            complexToUpdate.Latitude = dto.Latitude;
            complexToUpdate.Longitude = dto.Longitude;
            complexToUpdate.UpdatedAt = DateTime.UtcNow;

            // Xử lý ảnh mới nếu có
            if (dto.NewMainImageFile != null && dto.NewMainImageFile.Length > 0)
            {
                // Tùy chọn: Xóa ảnh cũ trước khi tải lên ảnh mới
                if (!string.IsNullOrEmpty(complexToUpdate.MainImageCloudinaryPublicId))
                {
                    await _imageUploadService.DeleteImageAsync(complexToUpdate.MainImageCloudinaryPublicId);
                    _logger.LogInformation("Old image {PublicId} deleted for complex {ComplexId}.", complexToUpdate.MainImageCloudinaryPublicId, dto.Id);
                }

                // Tải ảnh mới lên
                using var memoryStream = new MemoryStream();
                await dto.NewMainImageFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                var imageInput = new ImageInputDto
                {
                    Content = memoryStream,
                    FileName = dto.NewMainImageFile.FileName,
                    ContentType = dto.NewMainImageFile.ContentType
                };
                string folderName = $"court_complexes/{dto.Id}";
                var uploadResult = await _imageUploadService.UploadImageAsync(imageInput, folderName);

                if (uploadResult.Success)
                {
                    complexToUpdate.MainImageCloudinaryPublicId = uploadResult.PublicId;
                    complexToUpdate.MainImageCloudinaryUrl = uploadResult.Url;
                }
                else
                {
                    errors.Add("Lỗi tải ảnh mới: " + uploadResult.ErrorMessage);
                    // Quyết định xem có dừng lại nếu lỗi ảnh không. Hiện tại, các thay đổi khác vẫn được lưu.
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Complex {ComplexId} updated successfully by user {OwnerUserId}", dto.Id, ownerUserId);
                return (true, errors.Any() ? errors : null); // Thành công, nhưng có thể có lỗi không nghiêm trọng (ví dụ lỗi ảnh)
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while updating complex {ComplexId}", dto.Id);
                errors.Add("Lỗi khi lưu vào cơ sở dữ liệu.");
                return (false, errors);
            }
        }
        public async Task<CourtComplex> GetCourtComplexByIdAsync(int complexId, string ownerUserId)
        {
            var complex = await _context.CourtComplexes.FindAsync(complexId);
            if (complex == null || complex.OwnerUserId != ownerUserId)
            {
                return null; // Không tìm thấy hoặc không có quyền
            }
            return complex;
        }

        ////Dat
        public async Task<IEnumerable<CourtComplex>> GetCourtComplexesAsync()
        {
            return await _context.CourtComplexes
                .Include(cc => cc.OwnerUser)
                .ThenInclude(u => u.UserProfile)
                .ToListAsync();
        }

        public async Task<IEnumerable<CourtComplex>> SearchCourtComplexesAsync(string searchTerm)
        {
            var courtComplexes = await GetCourtComplexesAsync();
            if (string.IsNullOrEmpty(searchTerm))
            {
                return courtComplexes;
            }

            searchTerm = searchTerm.ToLower();
            return courtComplexes.Where(cc =>
                cc.Name.ToLower().Contains(searchTerm) ||
                cc.Address.ToLower().Contains(searchTerm) ||
                cc.City.ToLower().Contains(searchTerm));
        }

        public async Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId)
        {
            return await _context.CourtComplexes
                .Include(cc => cc.OwnerUser)
                .ThenInclude(u => u.UserProfile)
                .Include(cc => cc.Courts)
                .ThenInclude(c => c.SportType)
                .FirstOrDefaultAsync(cc => cc.CourtComplexId == courtComplexId);
        }

        public async Task UpdateCourtComplexAsync(CourtComplex courtComplex)
        {
            _context.CourtComplexes.Update(courtComplex);
            await _context.SaveChangesAsync();
        }

        public async Task<CourtComplexDetailDto?> GetDetailAsync(int complexId, DateOnly? date, CancellationToken ct = default)
        {
            int? dow = date.HasValue ? (int)date.Value.DayOfWeek : null;

            return await _context.CourtComplexes
                .AsNoTracking()
                .Where(c => c.CourtComplexId == complexId &&
                            
                            c.IsActiveByOwner )
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

