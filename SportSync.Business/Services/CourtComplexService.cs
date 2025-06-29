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
                .Include(c => c.SportType)  
                .Include(c => c.CourtComplexAmenities)  
                    .ThenInclude(cca => cca.Amenity)  
                .Include(c => c.Courts)  
                    .ThenInclude(co => co.HourlyPriceRates) 
                .FirstOrDefaultAsync(c => c.CourtComplexId == complexId, ct);

            if (courtComplex == null) return null;  
            var thumbnail = string.IsNullOrWhiteSpace(courtComplex.MainImageCloudinaryUrl)
                   ? "/assets/sportsync-background.png"             
                   : courtComplex.MainImageCloudinaryUrl;
            // Lấy thông tin tiện nghi cho khu phức hợp
            var amenities = courtComplex.CourtComplexAmenities
                .Where(cca => cca.Amenity != null)
                .Select(cca => new AmenityDto(cca.Amenity.Name)) 
                .ToList();

            // Lấy danh sách sân trong khu phức hợp và các giờ giá
            var courts = courtComplex.Courts
                .Select(co => new CourtWithSlotsDto
                {
                    CourtId = co.CourtId,
                    Name = co.Name,
                    SportTypeName = courtComplex.SportType.Name,  
                    HourlyPriceRates = co.HourlyPriceRates
                        .Select(hr => new HourlyPriceRateDto
                        {
                            HourlyPriceRateId = hr.HourlyPriceRateId,
                            Start = TimeOnly.FromTimeSpan(hr.StartTime),  
                            End = TimeOnly.FromTimeSpan(hr.EndTime),  
                            PricePerHour = hr.PricePerHour  
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
                Description = courtComplex.Description,  
                ContactPhoneNumber = courtComplex.ContactPhoneNumber,  
                ContactEmail = courtComplex.ContactEmail,  
                SportTypeName = courtComplex.SportType.Name, 
                GoogleMapsLink = courtComplex.GoogleMapsLink,
                MainImageUrl = thumbnail,
                Amenities = amenities,  
                Courts = courts  
            };
        }

        public async Task<IReadOnlyList<CourtComplexResultDto>> SearchAsync(
                    CourtSearchRequest rq, CancellationToken ct = default)
        {
          
            var complexesQ = _context.CourtComplexes
                .AsNoTracking()
                .Include(c => c.SportType)
                .Include(c => c.CourtComplexAmenities)
                    .ThenInclude(cca => cca.Amenity)
                .Include(c => c.Courts)
                    .ThenInclude(co => co.HourlyPriceRates)
                .Where(c => c.IsActiveByOwner);

          
            if (rq.SportTypeId is not null)
                complexesQ = complexesQ.Where(c => c.SportTypeId == rq.SportTypeId);

            if (!string.IsNullOrWhiteSpace(rq.City))
            {
                var city = rq.City.Trim().ToLower();
                complexesQ = complexesQ.Where(c =>
                    c.City != null && EF.Functions.Like(c.City.ToLower(), $"%{city}%"));
            }

            if (!string.IsNullOrWhiteSpace(rq.District))
            {
                var district = rq.District.Trim().ToLower();
                complexesQ = complexesQ.Where(c =>
                    c.District != null && EF.Functions.Like(c.District.ToLower(), $"%{district}%"));
            }

            var complexes = await complexesQ.ToListAsync(ct);

        
            var result = complexes.Select(cpx => new CourtComplexResultDto
            {
                ComplexId = cpx.CourtComplexId,
                Name = cpx.Name,
                Address = cpx.Address,
                SportTypeName = cpx.SportType.Name,
                ThumbnailUrl = cpx.MainImageCloudinaryUrl,
                Amenities = cpx.CourtComplexAmenities
                             .Select(cca => new AmenityDto(cca.Amenity!.Name))
                             .ToList(),

                Courts = cpx.Courts
                        .Where(co => co.StatusByOwner == CourtStatusByOwner.Available && co.HourlyPriceRates.Any())
                        .Take(2)                                       
                        .Select(co => new CourtWithSlotsDto
                        {
                            CourtId = co.CourtId,
                            Name = co.Name,

                            HourlyPriceRates = co.HourlyPriceRates          
                                                 .OrderBy(hr => hr.StartTime)
                                                 .Take(2)
                                                 .Select(hr => new HourlyPriceRateDto
                                                 {
                                                     HourlyPriceRateId = hr.HourlyPriceRateId,
                                                     Start = TimeOnly.FromTimeSpan(hr.StartTime),
                                                     End = TimeOnly.FromTimeSpan(hr.EndTime),
                                                     PricePerHour = hr.PricePerHour
                                                 })
                                                 .ToList()
                        })
                        .ToList()
            })
            .ToList();

            return result;
        }
    }
}

