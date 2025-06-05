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

        Task<IReadOnlyList<CourtComplexResultDto>> SearchNearbyAsync(
    double userLat, double userLng,
    double radiusKm = 10,
    CourtSearchRequest rq = null,
    CancellationToken ct = default);
    }
        
}
