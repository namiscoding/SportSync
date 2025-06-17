using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class Product
    {
        public int ProductId { get; set; } 

        public int CourtComplexId { get; set; } = default!; 
        public CourtComplex CourtComplex { get; set; } = default!;

        public int ProductCategoryId { get; set; } = default!;
        public ProductCategory ProductCategory { get; set; } = default!;

        public string Name { get; set; } = default!; 

        public decimal UnitPrice { get; set; } 

        public ProductType ProductType { get; set; } 

        public bool IsActive { get; set; } 

        public int? StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<BookingProduct>? BookingProducts { get; set; }

        public Product()
        {
            BookingProducts = new HashSet<BookingProduct>();
        }
    }

}
