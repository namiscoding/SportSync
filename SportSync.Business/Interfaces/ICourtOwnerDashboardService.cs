using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Business.Dtos.OwnerDashboard;

namespace SportSync.Business.Interfaces
{
    public interface ICourtOwnerDashboardService
    {
        Task<CourtOwnerDashboardDto> GetDashboardDataAsync(string ownerUserId);
    }
}
