using SportSync.Data.Entities;
using SportSync.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportSync.Data.Repositories
{
    public class SportTypeService : ISportTypeService
    {
        private readonly ApplicationDbContext _dbContext;

        public SportTypeService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<SportType>> GetSportTypesAsync()
        {
            return await _dbContext.SportTypes.ToListAsync();
        }

        public async Task<SportType> GetSportTypeByIdAsync(int sportTypeId)
        {
            return await _dbContext.SportTypes.FindAsync(sportTypeId);
        }

        public async Task AddSportTypeAsync(SportType sportType)
        {
            await _dbContext.SportTypes.AddAsync(sportType);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateSportTypeAsync(SportType sportType)
        {
            _dbContext.SportTypes.Update(sportType);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteSportTypeAsync(int sportTypeId)
        {
            var sportType = await _dbContext.SportTypes.FindAsync(sportTypeId);
            if (sportType != null)
            {
                _dbContext.SportTypes.Remove(sportType);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}