using SportSync.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportSync.Business.Interfaces
{
    public interface IApplicationUserService
    {
        Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role);
        Task<IEnumerable<ApplicationUser>> SearchUsersByRoleAsync(string role, string searchTerm);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task UpdateUserAsync(ApplicationUser user);
    }
}