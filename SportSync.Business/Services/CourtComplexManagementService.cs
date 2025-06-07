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

        public async Task ApproveCourtComplexAsync(int courtComplexId, string adminId)
        {
            var courtComplex = await _courtComplexService.GetCourtComplexByIdAsync(courtComplexId);
            if (courtComplex == null)
            {
                throw new Exception("Court complex not found.");
            }

            courtComplex.ApprovalStatus = ApprovalStatus.Approved;
            courtComplex.ApprovedByAdminId = adminId;
            courtComplex.ApprovedAt = DateTime.UtcNow;
            courtComplex.IsActiveByAdmin = true;

            await _courtComplexService.UpdateCourtComplexAsync(courtComplex);
        }

        public async Task RejectCourtComplexAsync(int courtComplexId, string adminId, string rejectionReason)
        {
            var courtComplex = await _courtComplexService.GetCourtComplexByIdAsync(courtComplexId);
            if (courtComplex == null)
            {
                throw new Exception("Court complex not found.");
            }

            courtComplex.ApprovalStatus = ApprovalStatus.RejectedByAdmin;
            courtComplex.ApprovedByAdminId = adminId;
            courtComplex.ApprovedAt = DateTime.UtcNow;
            courtComplex.RejectionReason = rejectionReason;
            courtComplex.IsActiveByAdmin = false;

            await _courtComplexService.UpdateCourtComplexAsync(courtComplex);
        }

        public async Task ToggleCourtComplexStatusAsync(int courtComplexId)
        {
            var courtComplex = await _courtComplexService.GetCourtComplexByIdAsync(courtComplexId);
            if (courtComplex == null)
            {
                throw new Exception("Court complex not found.");
            }

            courtComplex.IsActiveByAdmin = !courtComplex.IsActiveByAdmin;
            courtComplex.UpdatedAt = DateTime.UtcNow;

            await _courtComplexService.UpdateCourtComplexAsync(courtComplex);
        }
    }
}