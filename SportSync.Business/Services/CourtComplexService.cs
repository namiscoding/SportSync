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
                .FirstOrDefaultAsync(cc => cc.CourtComplexId == courtComplexId);
        }

        public async Task UpdateCourtComplexAsync(CourtComplex courtComplex)
        {
            _context.CourtComplexes.Update(courtComplex);
            await _context.SaveChangesAsync();
        }


        public Task<(bool Success, CourtComplex CreatedComplex, IEnumerable<string> Errors)> CreateCourtComplexAsync(CreateCourtComplexDto dto, string ownerUserId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, IEnumerable<string> Errors)> UpdateCourtComplexAsync(UpdateCourtComplexDto dto, string ownerUserId)
        {
            throw new NotImplementedException();
        }


        public async Task<CourtComplexDetailDto?> GetDetailAsync(int complexId, CancellationToken ct = default)
        {
            // Tìm kiếm CourtComplex theo CourtComplexId
            var courtComplex = await _context.CourtComplexes
                .AsNoTracking()
                .Include(c => c.SportType)  // Bao gồm loại thể thao
                .Include(c => c.CourtComplexAmenities)  // Bao gồm tiện nghi của khu phức hợp
                    .ThenInclude(cca => cca.Amenity)  // Bao gồm các tiện nghi
                .Include(c => c.Courts)  // Bao gồm các sân trong khu phức hợp
                    .ThenInclude(co => co.HourlyPriceRates)  // Bao gồm thông tin khung giờ của các sân
                .FirstOrDefaultAsync(c => c.CourtComplexId == complexId, ct);

            if (courtComplex == null) return null;  // Trả về null nếu không tìm thấy khu phức hợp

            // Lấy thông tin tiện nghi cho khu phức hợp
            var amenities = courtComplex.CourtComplexAmenities
                .Where(cca => cca.Amenity != null)
                .Select(cca => new AmenityDto(cca.Amenity.Name))  // Chuyển thành DTO Amenity
                .ToList();

            // Lấy danh sách sân trong khu phức hợp và các giờ giá
            var courts = courtComplex.Courts
                .Select(co => new CourtWithSlotsDto
                {
                    CourtId = co.CourtId,
                    Name = co.Name,
                    SportTypeName = courtComplex.SportType.Name,  // Lấy tên loại thể thao từ khu phức hợp
                    StatusByOwner = co.StatusByOwner.ToString(),  // Trạng thái của sân

                    HourlyPriceRates = co.HourlyPriceRates
                        .Select(hr => new HourlyPriceRateDto
                        {
                            HourlyPriceRateId = hr.HourlyPriceRateId,
                            Start = TimeOnly.FromTimeSpan(hr.StartTime),  // Chuyển từ TimeSpan sang TimeOnly
                            End = TimeOnly.FromTimeSpan(hr.EndTime),  // Chuyển từ TimeSpan sang TimeOnly
                            PricePerHour = hr.PricePerHour  // Giá theo giờ
                        })
                        .ToList()
                })
                .ToList();

            // Trả về DTO chi tiết khu phức hợp
            return new CourtComplexDetailDto
            {
                ComplexId = courtComplex.CourtComplexId,
                Name = courtComplex.Name,
                Address = courtComplex.Address,
                Description = courtComplex.Description,  // Mô tả khu phức hợp
                ContactPhoneNumber = courtComplex.ContactPhoneNumber,  // Số điện thoại liên hệ
                ContactEmail = courtComplex.ContactEmail,  // Email liên hệ
                SportTypeName = courtComplex.SportType.Name,  // Tên loại thể thao
                GoogleMapsLink = courtComplex.GoogleMapsLink,  // Liên kết đến Google Maps
                Amenities = amenities,  // Tiện nghi của khu phức hợp
                Courts = courts  // Danh sách sân và khung giờ
            };
        }

        public async Task<IReadOnlyList<CourtComplexResultDto>> SearchAsync(CourtSearchRequest rq, CancellationToken ct = default)
        {
            var complexesQ = _context.CourtComplexes
                .AsNoTracking()
                .Include(c => c.SportType)
                .Include(c => c.Courts)
                .Where(c => c.IsActiveByOwner);

            // Lọc theo SportType nếu có
            if (rq.SportTypeId.HasValue)
            {
                complexesQ = complexesQ.Where(c => c.SportTypeId == rq.SportTypeId);
            }

            // Lọc theo Thành phố nếu có
            if (!string.IsNullOrWhiteSpace(rq.City))
            {
                var city = rq.City.Trim().ToLower();
                complexesQ = complexesQ.Where(c =>
                    c.City != null &&
                    EF.Functions.Like(c.City.ToLower(), $"%{city}%"));
            }

            // Lọc theo Quận huyện nếu có
            if (!string.IsNullOrWhiteSpace(rq.District))
            {
                var district = rq.District.Trim().ToLower();
                complexesQ = complexesQ.Where(c =>
                    c.District != null &&
                    EF.Functions.Like(c.District.ToLower(), $"%{district}%"));
            }

            var raw = await complexesQ
                .Select(cpx => new
                {
                    cpx.CourtComplexId,
                    cpx.Name,
                    cpx.Address,
                    cpx.GoogleMapsLink,
                    SportTypeName = cpx.SportType.Name,
                    Courts = cpx.Courts
                        .Where(co => co.StatusByOwner == CourtStatusByOwner.Available)
                        .Take(2)
                        .Select(co => new
                        {
                            co.CourtId,
                            co.Name
                        })
                })
                .ToListAsync(ct);

            var list = raw
                .Select(c => new CourtComplexResultDto
                {
                    ComplexId = c.CourtComplexId,
                    Name = c.Name,
                    Address = c.Address,
                    GoogleMapsLink = c.GoogleMapsLink,
                    SportTypeName = c.SportTypeName,
                    Courts = c.Courts
                        .Select(co => new CourtWithSlotsDto
                        {
                            CourtId = co.CourtId,
                            Name = co.Name,

                        })
                })
                .ToList();

            return list;
        }
    }
}

