using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class CourtManagementService
    {
        private readonly ICourtService _courtService;

        public CourtManagementService(ICourtService courtRepository)
        {
            _courtService = courtRepository;
        }

        public async Task<Court> GetCourtByIdAsync(int courtId)
        {
            return await _courtService.GetCourtByIdAsync(courtId);
        }

        public async Task ToggleCourtStatusAsync(int courtId)
        {
            var court = await _courtService.GetCourtByIdAsync(courtId);
            if (court == null)
            {
                throw new Exception("Court not found.");
            }

            // Toggle giữa Available (0) và Suspended (1)
            court.StatusByOwner = court.StatusByOwner == CourtStatusByOwner.Available 
                ? CourtStatusByOwner.Suspended 
                : CourtStatusByOwner.Available;
            court.UpdatedAt = DateTime.UtcNow;

            await _courtService.UpdateCourtAsync(court);
        }
    }
}