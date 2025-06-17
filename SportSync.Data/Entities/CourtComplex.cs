using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class CourtComplex
    {
        public int CourtComplexId { get; set; } // PK, IDENTITY(1,1) handled in DbContext

        public string OwnerUserId { get; set; } = default!; // FK to ApplicationUser
        public ApplicationUser OwnerUser { get; set; } = default!;

        public int SportTypeId { get; set; } // FK to SportType
        public SportType SportType { get; set; } = default!;

        public string Name { get; set; } = default!; // Required, StringLength handled in DbContext

        public string Address { get; set; } = default!; // Required, StringLength handled in DbContext

        public string City { get; set; } = default!; // Required, StringLength handled in DbContext

        public string District { get; set; } = default!; // Required, StringLength handled in DbContext

        public string? Ward { get; set; }

        public string? GoogleMapsLink { get; set; }

        public string? Description { get; set; }

        public string? MainImageCloudinaryPublicId { get; set; }
        public string? MainImageCloudinaryUrl { get; set; }

        public string? ContactPhoneNumber { get; set; }

        public string? ContactEmail { get; set; }

        public TimeSpan? DefaultOpeningTime { get; set; }

        public TimeSpan? DefaultClosingTime { get; set; }

        public bool IsActiveByOwner { get; set; } // Default value handled in DbContext

        public string? BankCode { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }

        public DateTime CreatedAt { get; set; } // Default value handled in DbContext
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Court>? Courts { get; set; }
        public ICollection<CourtComplexImage>? CourtComplexImages { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<CourtComplexAmenity>? CourtComplexAmenities { get; set; }
        public ICollection<Product>? Products { get; set; }

        public CourtComplex()
        {
            Courts = new HashSet<Court>();
            CourtComplexImages = new HashSet<CourtComplexImage>();
            Bookings = new HashSet<Booking>();
            CourtComplexAmenities = new HashSet<CourtComplexAmenity>();
            Products = new HashSet<Product>();
        }
    }
}
