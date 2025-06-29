using SportSync.Data.Entities;
using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var standardCourtOwners = await _userService.GetUsersByRoleAsync("StandardCourtOwner");
            var proCourtOwners = await _userService.GetUsersByRoleAsync("ProCourtOwner");
            return standardCourtOwners.Concat(proCourtOwners);
        }

        public async Task<IEnumerable<ApplicationUser>> SearchCourtOwnersAsync(string searchTerm, string selectedRole)
        {
            IEnumerable<ApplicationUser> courtOwners;

            // Lọc theo vai trò
            if (selectedRole == "StandardCourtOwner")
            {
                courtOwners = await _userService.SearchUsersByRoleAsync("StandardCourtOwner", searchTerm);
            }
            else if (selectedRole == "ProCourtOwner")
            {
                courtOwners = await _userService.SearchUsersByRoleAsync("ProCourtOwner", searchTerm);
            }
            else
            {
                var standardCourtOwners = await _userService.SearchUsersByRoleAsync("StandardCourtOwner", searchTerm);
                var proCourtOwners = await _userService.SearchUsersByRoleAsync("ProCourtOwner", searchTerm);
                courtOwners = standardCourtOwners.Concat(proCourtOwners);
            }

            return courtOwners;
        }

        public async Task ApproveCourtOwnerAsync(string userId, string adminId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Chủ sân không tìm thấy.");
            }

            user.UserProfile.AccountStatusByAdmin = (int)AccountStatus.Active;
            await _userService.UpdateUserAsync(user);
        }

        public async Task RejectCourtOwnerAsync(string userId, string adminId, string rejectionReason)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Chủ sân không tìm thấy.");
            }

            user.UserProfile.AccountStatusByAdmin = AccountStatus.SuspendedByAdmin;
            await _userService.UpdateUserAsync(user);
        }

        public async Task ToggleCourtOwnerAccountStatusAsync(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Chủ sân không tìm thấy.");
            }

            user.UserProfile.AccountStatusByAdmin = user.UserProfile.AccountStatusByAdmin == AccountStatus.Active
                ? AccountStatus.SuspendedByAdmin
                : AccountStatus.Active;

            await _userService.UpdateUserAsync(user);
        }
    }
}