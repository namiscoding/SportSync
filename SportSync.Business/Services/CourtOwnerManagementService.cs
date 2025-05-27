using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SportSync.Business.Interfaces;

namespace SportSync.Business.Services
{
    public class CourtOwnerManagementService
    {
        private readonly IApplicationUserService _userService;

        public CourtOwnerManagementService(IApplicationUserService userRepository)
        {
            _userService = userRepository;
        }

        public async Task<IEnumerable<ApplicationUser>> GetCourtOwnersAsync()
        {
            return await _userService.GetUsersByRoleAsync("CourtOwner");
        }

        public async Task<IEnumerable<ApplicationUser>> SearchCourtOwnersAsync(string searchTerm)
        {
            return await _userService.SearchUsersByRoleAsync("CourtOwner", searchTerm);
        }

        public async Task ApproveCourtOwnerAsync(string userId, string adminId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Court owner not found.");
            }

            user.UserProfile.AccountStatusByAdmin = AccountStatus.Active;
            await _userService.UpdateUserAsync(user);
        }

        public async Task RejectCourtOwnerAsync(string userId, string adminId, string rejectionReason)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Court owner not found.");
            }

            user.UserProfile.AccountStatusByAdmin = AccountStatus.SuspendedByAdmin;
            await _userService.UpdateUserAsync(user);
        }

        public async Task ToggleCourtOwnerAccountStatusAsync(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Court owner not found.");
            }

            user.UserProfile.AccountStatusByAdmin = user.UserProfile.AccountStatusByAdmin == AccountStatus.Active
                ? AccountStatus.SuspendedByAdmin
                : AccountStatus.Active;

            await _userService.UpdateUserAsync(user);
        }
    }
}