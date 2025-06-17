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

        public Task<CourtComplexDetailDto?> GetDetailAsync(int complexId, DateOnly? date = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}

