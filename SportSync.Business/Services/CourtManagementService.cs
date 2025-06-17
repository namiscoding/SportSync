using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
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

    }
}