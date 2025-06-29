using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos;
using SportSync.Data.Entities;

namespace SportSync.Business.Interfaces
{
    public interface ICourtService
    {
        Task<Court> GetCourtByIdAsync(int courtId);
        Task UpdateCourtAsync(Court court);

        //Nam
        Task<IEnumerable<CourtOutputDto>> GetCourtsByComplexIdAsync(int courtComplexId);
        Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId); // Lấy thông tin Complex để hiển thị tên
        Task<CourtComplex> GetCourtComplexByIdAsync(int courtComplexId, string ownerUserId);
        Task<(bool Success, Court CreatedCourt, IEnumerable<string> Errors)> CreateCourtAsync(CreateCourtDto dto, string ownerUserId);
        Task<IEnumerable<SportType>> GetAllSportTypesAsync(); // Để lấy danh sách loại sân
        Task<IEnumerable<Amenity>> GetAllAmenitiesAsync(); // Để lấy danh sách tiện ích
        Task<(bool Success, string NewStatus, string ErrorMessage)> ToggleCourtStatusAsync(int courtId, string ownerUserId);
        Task<Court> GetCourtForEditAsync(int courtId, string ownerUserId); // Lấy entity để map sang ViewModel
        Task<(bool Success, IEnumerable<string> Errors)> UpdateCourtAsync(UpdateCourtDto dto, string ownerUserId);
        Task<CourtDetailDto?> GetCourtDetailAsync(int courtId, CancellationToken ct = default);
    }
}   
