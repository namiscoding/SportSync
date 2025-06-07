using SportSync.Data.Entities;
using SportSync.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SportSync.Business.Interfaces;
using SportSync.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SportSync.Business.Dtos;
using SportSync.Data.Enums;

namespace SportSync.Business.Services
{
    public class CourtService : ICourtService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<CourtService> _logger;

        public CourtService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IImageUploadService imageUploadService,
            ILogger<CourtService> logger)
        {
            _context = context;
            _userManager = userManager;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        public async Task<Court> GetCourtByIdAsync(int courtId)
        {
            return await _context.Courts
                .Include(c => c.CourtComplex)
                .Include(c => c.SportType)
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }

        public async Task UpdateCourtAsync(Court court)
        {
            _context.Courts.Update(court);
            await _context.SaveChangesAsync();
        }

        public async Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId)
        {
            return await _context.CourtComplexes.FindAsync(courtComplexId);
        }
        public async Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId, string ownerUserId)
        {
            var complex = await _context.CourtComplexes.FindAsync(courtComplexId);

            if (complex == null || complex.OwnerUserId != ownerUserId)
            {
                return null;
            }

            return complex;
        }

        public async Task<IEnumerable<SportType>> GetAllSportTypesAsync()
        {
            return await _context.SportTypes.Where(st => st.IsActive).OrderBy(st => st.Name).ToListAsync();
        }

        public async Task<IEnumerable<Amenity>> GetAllAmenitiesAsync()
        {
            return await _context.Amenities.Where(a => a.IsActive).OrderBy(a => a.Name).ToListAsync();
        }


        public async Task<IEnumerable<CourtOutputDto>> GetCourtsByComplexIdAsync(int courtComplexId)
        {
            return await _context.Courts
                .AsNoTracking() 
                .Where(c => c.CourtComplexId == courtComplexId)
                .Include(c => c.SportType)
                .Include(c => c.CourtAmenities).ThenInclude(ca => ca.Amenity)
                .Select(c => new CourtOutputDto
                {
                    CourtId = c.CourtId,
                    Name = c.Name,
                    SportTypeName = c.SportType.Name,
                    Description = c.Description,
                    DefaultSlotDurationMinutes = c.DefaultSlotDurationMinutes,
                    MainImageCloudinaryUrl = c.MainImageCloudinaryUrl,
                    AmenityNames = c.CourtAmenities.Select(ca => ca.Amenity.Name).ToList(),
                    CourtComplexId = c.CourtComplexId,
                    StatusByOwner = c.StatusByOwner
                })
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<(bool Success, Court CreatedCourt, IEnumerable<string> Errors)> CreateCourtAsync(CreateCourtDto dto, string ownerUserId)
        {
            var errors = new List<string>();
            _logger.LogInformation("Attempting to create court '{CourtName}' for complex {ComplexId} by user {OwnerUserId}", dto.Name, dto.CourtComplexId, ownerUserId);

            var courtComplex = await _context.CourtComplexes.FindAsync(dto.CourtComplexId);
            if (courtComplex == null || courtComplex.OwnerUserId != ownerUserId)
            {
                _logger.LogWarning("Court complex {ComplexId} not found or user {OwnerUserId} is not the owner.", dto.CourtComplexId, ownerUserId);
                errors.Add("Khu phức hợp sân không hợp lệ hoặc bạn không có quyền thêm sân vào đây.");
                return (false, null, errors);
            }

            var court = new Court
            {
                CourtComplexId = dto.CourtComplexId,
                Name = dto.Name,
                SportTypeId = dto.SportTypeId,
                Description = dto.Description,
                DefaultSlotDurationMinutes = dto.DefaultSlotDurationMinutes,
                AdvanceBookingDaysLimit = dto.AdvanceBookingDaysLimit,
                OpeningTime = dto.OpeningTime,
                ClosingTime = dto.ClosingTime,
                StatusByOwner = Data.Enums.CourtStatusByOwner.Available,
                IsActiveByAdmin = true, // Mặc định là true, Admin có thể thay đổi sau
                CreatedAt = DateTime.UtcNow
            };

            if (dto.MainImageFile != null && dto.MainImageFile.Length > 0)
            {
                _logger.LogInformation("Uploading main image for court: {FileName}", dto.MainImageFile.FileName);
                using var memoryStream = new MemoryStream();
                await dto.MainImageFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var imageInput = new ImageInputDto
                {
                    Content = memoryStream,
                    FileName = dto.MainImageFile.FileName,
                    ContentType = dto.MainImageFile.ContentType
                };
                string folderName = $"court_complexes/{dto.CourtComplexId}/courts/{Guid.NewGuid()}";
                var uploadResult = await _imageUploadService.UploadImageAsync(imageInput, folderName);

                if (uploadResult.Success)
                {
                    court.MainImageCloudinaryPublicId = uploadResult.PublicId;
                    court.MainImageCloudinaryUrl = uploadResult.Url;
                }
                else
                {
                    errors.Add("Lỗi tải ảnh sân: " + (uploadResult.ErrorMessage ?? "Không xác định"));
                }
            }

            // Xử lý tiện ích (Amenities)
            if (dto.SelectedAmenityIds != null && dto.SelectedAmenityIds.Any())
            {
                court.CourtAmenities = new List<CourtAmenity>();
                foreach (var amenityId in dto.SelectedAmenityIds)
                {
                    // Kiểm tra xem AmenityId có hợp lệ không (tồn tại trong DB)
                    var amenityExists = await _context.Amenities.AnyAsync(a => a.AmenityId == amenityId && a.IsActive);
                    if (amenityExists)
                    {
                        court.CourtAmenities.Add(new CourtAmenity { AmenityId = amenityId });
                    }
                    else
                    {
                        _logger.LogWarning("Attempted to add non-existent or inactive AmenityId {AmenityId} to court {CourtName}", amenityId, dto.Name);
                    }
                }
            }


            try
            {
                _context.Courts.Add(court);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Court '{CourtName}' (ID: {CourtId}) created successfully for complex {ComplexId}", court.Name, court.CourtId, court.CourtComplexId);
                return (true, court, errors.Any() ? errors : null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while creating court '{CourtName}'", dto.Name);
                errors.Add("Lỗi lưu thông tin sân vào cơ sở dữ liệu.");
                return (false, null, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic exception while creating court '{CourtName}'", dto.Name);
                errors.Add("Đã xảy ra lỗi không mong muốn khi tạo sân.");
                return (false, null, errors);
            }
        }
        public async Task<(bool Success, string NewStatus, string ErrorMessage)> ToggleCourtStatusAsync(int courtId, string ownerUserId)
        {
            var court = await _context.Courts
                                .Include(c => c.CourtComplex)
                                .FirstOrDefaultAsync(c => c.CourtId == courtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                _logger.LogWarning("Toggle status failed. Court {CourtId} not found or user {OwnerUserId} is not the owner.", courtId, ownerUserId);
                return (false, null, "Không tìm thấy sân hoặc bạn không có quyền thay đổi trạng thái.");
            }

            if (court.StatusByOwner == CourtStatusByOwner.Available)
            {
                court.StatusByOwner = CourtStatusByOwner.Suspended;
            }
            else 
            {
                court.StatusByOwner = CourtStatusByOwner.Available;
            }

            try
            {
                await _context.SaveChangesAsync();
                string newStatusText = court.StatusByOwner == CourtStatusByOwner.Available ? "Hoạt động" : "Ngưng hoạt động";
                _logger.LogInformation("Status for court {CourtId} toggled to {NewStatus} by user {OwnerUserId}.", courtId, court.StatusByOwner, ownerUserId);
                return (true, newStatusText, null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while toggling status for court {CourtId}.", courtId);
                return (false, null, "Lỗi khi cập nhật cơ sở dữ liệu.");
            }
        }
        public async Task<Court> GetCourtForEditAsync(int courtId, string ownerUserId)
        {
            var court = await _context.Courts
                .Include(c => c.CourtComplex)
                .Include(c => c.CourtAmenities) // Include các tiện ích hiện có
                .FirstOrDefaultAsync(c => c.CourtId == courtId);

            if (court == null || court.CourtComplex.OwnerUserId != ownerUserId)
            {
                _logger.LogWarning("GetCourtForEditAsync failed. Court {CourtId} not found or user {OwnerUserId} is not owner.", courtId, ownerUserId);
                return null;
            }

            return court;
        }

        public async Task<(bool Success, IEnumerable<string> Errors)> UpdateCourtAsync(UpdateCourtDto dto, string ownerUserId)
        {
            var errors = new List<string>();
            var courtToUpdate = await _context.Courts
                                    .Include(c => c.CourtComplex)
                                    .Include(c => c.CourtAmenities) // Include để có thể cập nhật tiện ích
                                    .FirstOrDefaultAsync(c => c.CourtId == dto.CourtId);

            if (courtToUpdate == null || courtToUpdate.CourtComplex.OwnerUserId != ownerUserId)
            {
                errors.Add("Không tìm thấy sân hoặc bạn không có quyền chỉnh sửa.");
                return (false, errors);
            }

            // Cập nhật các thuộc tính
            courtToUpdate.Name = dto.Name;
            courtToUpdate.SportTypeId = dto.SportTypeId;
            courtToUpdate.Description = dto.Description;
            courtToUpdate.DefaultSlotDurationMinutes = dto.DefaultSlotDurationMinutes;
            courtToUpdate.AdvanceBookingDaysLimit = dto.AdvanceBookingDaysLimit;
            courtToUpdate.OpeningTime = dto.OpeningTime;
            courtToUpdate.ClosingTime = dto.ClosingTime;
            courtToUpdate.UpdatedAt = DateTime.UtcNow;

            // Xử lý ảnh mới nếu có
            if (dto.NewMainImageFile != null && dto.NewMainImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(courtToUpdate.MainImageCloudinaryPublicId))
                {
                    await _imageUploadService.DeleteImageAsync(courtToUpdate.MainImageCloudinaryPublicId);
                }

                using var memoryStream = new MemoryStream();
                await dto.NewMainImageFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                var imageInput = new ImageInputDto { Content = memoryStream, FileName = dto.NewMainImageFile.FileName };
                var uploadResult = await _imageUploadService.UploadImageAsync(imageInput, $"court_complexes/{dto.CourtComplexId}/courts/{courtToUpdate.CourtId}");

                if (uploadResult.Success)
                {
                    courtToUpdate.MainImageCloudinaryPublicId = uploadResult.PublicId;
                    courtToUpdate.MainImageCloudinaryUrl = uploadResult.Url;
                }
                else { errors.Add("Lỗi tải ảnh mới: " + uploadResult.ErrorMessage); }
            }

            // Cập nhật tiện ích (Amenities)
            // Xóa các tiện ích cũ không còn được chọn, và thêm các tiện ích mới
            var currentAmenityIds = courtToUpdate.CourtAmenities.Select(ca => ca.AmenityId).ToList();
            var newAmenityIds = dto.SelectedAmenityIds ?? new List<int>();

            var amenitiesToRemove = courtToUpdate.CourtAmenities.Where(ca => !newAmenityIds.Contains(ca.AmenityId)).ToList();
            var amenityIdsToAdd = newAmenityIds.Where(id => !currentAmenityIds.Contains(id)).ToList();

            if (amenitiesToRemove.Any()) _context.CourtAmenities.RemoveRange(amenitiesToRemove);

            foreach (var amenityId in amenityIdsToAdd)
            {
                courtToUpdate.CourtAmenities.Add(new CourtAmenity { AmenityId = amenityId });
            }

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Court {CourtId} updated successfully by user {OwnerUserId}", dto.CourtId, ownerUserId);
                return (true, errors.Any() ? errors : null);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DbUpdateException while updating court {CourtId}", dto.CourtId);
                errors.Add("Lỗi khi lưu vào cơ sở dữ liệu.");
                return (false, errors);
            }
        }
    }
}
