using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportSync.Business.Interfaces;

namespace SportSync.Business.Services
{
    public class UserManagementService
    {
        private readonly IApplicationUserService _userService;

        public UserManagementService(IApplicationUserService userRepository)
        {
            _userService = userRepository;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await _userService.GetUsersByRoleAsync("Customer");
        }

        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string searchTerm)
        {
            return await _userService.SearchUsersByRoleAsync("Customer", searchTerm);
        }

        public async Task<IEnumerable<ApplicationUser>> GetCustomersAsync()
        {
            return await _userService.GetUsersByRoleAsync("Customer");
        }

        public async Task<IEnumerable<ApplicationUser>> SearchCustomersAsync(string searchTerm)
        {
            return await _userService.SearchUsersByRoleAsync("Customer", searchTerm);
        }

        public async Task ToggleUserAccountStatusAsync(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            user.UserProfile.AccountStatusByAdmin = user.UserProfile.AccountStatusByAdmin == AccountStatus.Active
                ? AccountStatus.SuspendedByAdmin
                : (int)AccountStatus.Active;

            await _userService.UpdateUserAsync(user);
        }
    }
}