using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos;
using SportSync.Data.Entities;

namespace SportSync.Business.Interfaces
{
    public interface ICourtComplexService
    {
        Task<IEnumerable<CourtComplex>> GetCourtComplexesByOwnerAsync(string ownerUserId);
        // **THAY ĐỔI VIEWMODEL THÀNH DTO**
        Task<(bool Success, CourtComplex CreatedComplex, IEnumerable<string> Errors)> CreateCourtComplexAsync(CreateCourtComplexDto dto, string ownerUserId);
        Task<(bool Success, IEnumerable<string> Errors)> UpdateCourtComplexAsync(UpdateCourtComplexDto dto, string ownerUserId);
        Task<CourtComplex> GetCourtComplexByIdAsync(int complexId, string ownerUserId);
        //Dat
        Task<IEnumerable<CourtComplex>> GetCourtComplexesAsync();
        Task<IEnumerable<CourtComplex>> SearchCourtComplexesAsync(string searchTerm);
        Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId);
        Task UpdateCourtComplexAsync(CourtComplex courtComplex);
        Task<CourtComplexDetailDto?> GetDetailAsync(int complexId, DateOnly? date = null, CancellationToken ct = default);


    }
}
