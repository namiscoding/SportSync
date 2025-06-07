using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos;

namespace SportSync.Business.Interfaces
{
    public interface ITimeSlotManagementService
    {
        Task<TimeSlotTemplateDto> GetTimeSlotTemplateForCourtAsync(int courtId, string ownerUserId);

        Task<(bool Success, string ErrorMessage)> UpdateBulkTimeSlotsAsync(BulkTimeSlotUpdateDto updateData, string ownerUserId);

    }
}
