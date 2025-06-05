using SportSync.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportSync.Data.Interfaces
{
    public interface ISportTypeService
    {
        Task<IEnumerable<SportType>> GetSportTypesAsync();
        Task<SportType> GetSportTypeByIdAsync(int sportTypeId);
        Task AddSportTypeAsync(SportType sportType);
        Task UpdateSportTypeAsync(SportType sportType);
        Task DeleteSportTypeAsync(int sportTypeId);
    }
}