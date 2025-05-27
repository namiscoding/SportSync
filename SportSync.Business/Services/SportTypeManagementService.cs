using SportSync.Data.Entities;
using SportSync.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class SportTypeManagementService
    {
        private readonly ISportTypeService _sportTypeService;

        public SportTypeManagementService(ISportTypeService sportTypeService)
        {
            _sportTypeService = sportTypeService;
        }

        public async Task<IEnumerable<SportType>> GetSportTypesAsync()
        {
            return await _sportTypeService.GetSportTypesAsync();
        }

        public async Task<SportType> GetSportTypeByIdAsync(int sportTypeId)
        {
            return await _sportTypeService.GetSportTypeByIdAsync(sportTypeId);
        }

        public async Task AddSportTypeAsync(string name, string description)
        {
            var sportType = new SportType
            {
                Name = name,
                Description = description,
                IsActive = true
            };
            await _sportTypeService.AddSportTypeAsync(sportType);
        }

        public async Task UpdateSportTypeAsync(int sportTypeId, string name, string description, bool isActive)
        {
            var sportType = await _sportTypeService.GetSportTypeByIdAsync(sportTypeId);
            if (sportType == null)
            {
                throw new Exception("Sport type not found.");
            }

            sportType.Name = name;
            sportType.Description = description;
            sportType.IsActive = isActive;
            await _sportTypeService.UpdateSportTypeAsync(sportType);
        }

        public async Task DeleteSportTypeAsync(int sportTypeId)
        {
            await _sportTypeService.DeleteSportTypeAsync(sportTypeId);
        }
    }
}