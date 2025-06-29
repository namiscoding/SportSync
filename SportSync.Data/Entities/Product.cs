using System;

namespace SportSync.Data.Entities
{
    public class Product
    {
        public int ProductId { get; set; } // PK, Identity
        public int CourtComplexId { get; set; } // FK, NOT NULL, ON DELETE CASCADE
        public int ProductCategoryId { get; set; } // FK, NOT NULL, ON DELETE RESTRICT
        public string Name { get; set; } // NOT NULL, MaxLength(255)
        public decimal UnitPrice { get; set; } // NOT NULL, DECIMAL(18,2)
        public int ProductType { get; set; } // NOT NULL
        public bool IsActive { get; set; } // NOT NULL, DEFAULT 1
        public int? StockQuantity { get; set; } // NULL
        public DateTime CreatedAt { get; set; } // NOT NULL, DEFAULT GETDATE()
        public DateTime? UpdatedAt { get; set; } // NULL

        // Navigation Properties
        public virtual CourtComplex CourtComplex { get; set; }
        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ICollection<BookingProduct> BookingProducts { get; set; }

        public Product()
        {
            BookingProducts = new HashSet<BookingProduct>();
        }
    }
}