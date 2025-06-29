using System.Collections.Generic;

namespace SportSync.Data.Entities
{
    public class ProductCategory
    {
        public int ProductCategoryId { get; set; } // PK, Identity
        public string Name { get; set; } // UNIQUE, NOT NULL, MaxLength(150)
        public bool IsActive { get; set; } // NOT NULL, DEFAULT 1

        // Navigation Property
        public virtual ICollection<Product> Products { get; set; }

        public ProductCategory()
        {
            Products = new HashSet<Product>();
        }
    }
}