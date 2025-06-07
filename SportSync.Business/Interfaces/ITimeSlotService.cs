using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos;

namespace SportSync.Business.Interfaces
{
    public interface ITimeSlotService
    {
        Task<IEnumerable<TimeSlotOutputDto>> GetTimeSlotsByCourtIdAsync(int courtId);
        Task<(bool Success, IEnumerable<string> Errors)> CreateTimeSlotAsync(CreateTimeSlotDto dto, string ownerUserId);
    }
}
