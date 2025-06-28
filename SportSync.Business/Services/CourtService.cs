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

        public Task<Court> GetCourtByIdAsync(int courtId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CourtOutputDto>> GetCourtsByComplexIdAsync(int courtComplexId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, Court CreatedCourt, IEnumerable<string> Errors)> CreateCourtAsync(CreateCourtDto dto, string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<Court> GetCourtForEditAsync(int courtId, string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, IEnumerable<string> Errors)> UpdateCourtAsync(UpdateCourtDto dto, string ownerUserId)
        {
            throw new NotImplementedException();
        }
    }
}
