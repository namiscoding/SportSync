using SportSync.Business.Dtos;
using SportSync.Web.Models.ViewModels.Court;
using System;
using System.Collections.Generic;

namespace SportSync.Web.Models.ViewModels
{
    public class CourtComplexDetailViewModel
    {
        public int CourtComplexId { get; set; }
        public string Name { get; set; }
        public string OwnerPhoneNumber { get; set; }
        public string OwnerFullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string Description { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string ContactEmail { get; set; }
        public TimeOnly? DefaultOpeningTime { get; set; } 
        public TimeOnly? DefaultClosingTime { get; set; } 
        public string ApprovalStatus { get; set; }
        public bool IsActiveByAdmin { get; set; }
        public string RejectionReason { get; set; }

        // Các trường mới
        public List<AmenityDto> Amenities { get; set; } = new List<AmenityDto>();  
        public List<CourtViewModel> Courts { get; set; } = new List<CourtViewModel>(); 
    }
}