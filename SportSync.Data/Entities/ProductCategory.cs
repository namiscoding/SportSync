using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class ProductCategory
    {
        public int ProductCategoryId { get; set; }

        public string Name { get; set; } = default!; 

        public bool IsActive { get; set; } 

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product>? Products { get; set; }
    }
}
