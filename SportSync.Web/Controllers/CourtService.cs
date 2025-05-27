using SportSync.Data.Entities;
using SportSync.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SportSync.Business.Interfaces;

namespace SportSync.Data.Repositories
{
    public class CourtService : ICourtService
    {
        private readonly ApplicationDbContext _dbContext;

        public CourtService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Court> GetCourtByIdAsync(int courtId)
        {
            return await _dbContext.Courts
                .Include(c => c.CourtComplex)
                .Include(c => c.SportType)
                .FirstOrDefaultAsync(c => c.CourtId == courtId);
        }

        public async Task UpdateCourtAsync(Court court)
        {
            _dbContext.Courts.Update(court);
            await _dbContext.SaveChangesAsync();
        }
    }
}