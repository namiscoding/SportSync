using SportSync.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SportSync.Business.Interfaces;
using SportSync.Data;

namespace SportSync.Business.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ApplicationUserService(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            foreach (var user in users)
            {
                await _dbContext.Entry(user).Reference(u => u.UserProfile).LoadAsync();
            }
            return users;
        }

        public async Task<IEnumerable<ApplicationUser>> SearchUsersByRoleAsync(string role, string searchTerm)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            foreach (var user in users)
            {
                await _dbContext.Entry(user).Reference(u => u.UserProfile).LoadAsync();
            }

            if (string.IsNullOrEmpty(searchTerm))
            {
                return users;
            }

            searchTerm = searchTerm.ToLower();
            return users.Where(u => u.UserProfile?.FullName?.ToLower().Contains(searchTerm) ?? false);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _dbContext.Entry(user).Reference(u => u.UserProfile).LoadAsync();
            }
            return user;
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userManager.UpdateAsync(user);
        }
    }
}