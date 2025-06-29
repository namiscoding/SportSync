
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
        public string GoogleMapsLink { get; set; }
        public string Description { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string ContactEmail { get; set; }
        public TimeOnly? DefaultOpeningTime { get; set; }
        public TimeOnly? DefaultClosingTime { get; set; }
        public string SportTypeName { get; set; }
        public bool IsActiveByOwner { get; set; }
        public string MainImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<CourtViewModel> Courts { get; set; }
    }
}
