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
    }
}
