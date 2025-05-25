using SportSync.Business.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Interfaces
{
    public interface ICourtSearchService
    {
        Task<IReadOnlyList<CourtComplexResultDto>> SearchAsync(
    CourtSearchRequest request,
    CancellationToken ct = default);
    }
}
