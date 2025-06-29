using System;

namespace SportSync.Data.Entities
{
    public class BookingProduct
    {
        public long BookingProductId { get; set; } // PK, BIGINT, Identity
        public long BookingId { get; set; } // FK, NOT NULL, ON DELETE CASCADE
        public int ProductId { get; set; } // FK, NOT NULL, ON DELETE RESTRICT
        public int Quantity { get; set; } // NOT NULL
        public decimal UnitPriceAtTimeOfAddition { get; set; } // NOT NULL, DECIMAL(18,2)
        public DateTime? RentalStartTime { get; set; } // NULL
        public DateTime? RentalEndTime { get; set; } // NULL
        public decimal Subtotal { get; set; } // NOT NULL, DECIMAL(18,2)
        public DateTime AddedAt { get; set; } // NOT NULL, DEFAULT GETDATE()

        // Navigation Properties
        public virtual Booking Booking { get; set; }
        public virtual Product Product { get; set; }
    }
}