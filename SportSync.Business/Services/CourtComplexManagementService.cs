using SportSync.Business.Interfaces;
using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class CourtComplexManagementService
    {
        private readonly ICourtComplexService _courtComplexService;

        public CourtComplexManagementService(ICourtComplexService courtComplexService)
        {
            _courtComplexService = courtComplexService;
        }

        public async Task<IEnumerable<CourtComplex>> GetCourtComplexesAsync()
        {
            return await _courtComplexService.GetCourtComplexesAsync();
        }

        public async Task<IEnumerable<CourtComplex>> SearchCourtComplexesAsync(string searchTerm)
        {
            return await _courtComplexService.SearchCourtComplexesAsync(searchTerm);
        }

        public async Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId)
        {
            return await _courtComplexService.GetCourtComplexByIdAsync(courtComplexId);
        }
    }
}