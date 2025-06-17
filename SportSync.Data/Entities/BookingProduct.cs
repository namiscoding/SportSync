using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class BookingProduct
    {
        public long BookingProductId { get; set; } 

        public long BookingId { get; set; } = default!;
        public Booking Booking { get; set; } = default!;

        public int ProductId { get; set; } = default!; 
        public Product Product { get; set; } = default!;

        public int Quantity { get; set; } 
        public decimal UnitPriceAtTimeOfAddition { get; set; } 

        public DateTime? RentalStartTime { get; set; }
        public DateTime? RentalEndTime { get; set; }

        public decimal Subtotal { get; set; } 

        public DateTime AddedAt { get; set; } 
    }
}
