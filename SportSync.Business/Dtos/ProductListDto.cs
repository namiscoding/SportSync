using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed class ProductListDto
    {
        public int ProductId { get; init; }
        public string Name { get; init; } = null!;
        public string CategoryName { get; init; } = null!;
        public ProductType ProductType { get; init; }   // enum (Consumable | Rental …)
        public decimal UnitPrice { get; init; }
        public int? StockQuantity { get; init; }     // null = không khống chế
    }
}
