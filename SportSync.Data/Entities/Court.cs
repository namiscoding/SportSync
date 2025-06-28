using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class Court
    {
        public int CourtId { get; set; } // PK, IDENTITY(1,1) handled in DbContext

        public int CourtComplexId { get; set; } // FK to CourtComplex
        public CourtComplex CourtComplex { get; set; } = default!;

        public string Name { get; set; } = default!; // Required, StringLength handled in DbContext

        public string? Description { get; set; }

        public CourtStatusByOwner StatusByOwner { get; set; } // Default value handled in DbContext

        public DateTime CreatedAt { get; set; } // Default value handled in DbContext
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<HourlyPriceRate>? HourlyPriceRates { get; set; }
        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<BlockedCourtSlot>? BlockedCourtSlots { get; set; }

        public Court()
        {
            HourlyPriceRates = new HashSet<HourlyPriceRate>();
            Bookings = new HashSet<Booking>();
            BlockedCourtSlots = new HashSet<BlockedCourtSlot>();
        }
    }
}
