
﻿using SportSync.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed class CourtDetailDto
    {
        // ---- Court ----
        public int CourtId { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public CourtStatusByOwner StatusByOwner { get; init; }

        // ---- Thuộc Court-Complex ----
        public int ComplexId { get; init; }
        public string ComplexName { get; init; } = null!;
        public string ComplexAddress { get; init; } = null!;

        public string SportTypeName { get; init; } = null!;

        // Khung giờ giá
        public IReadOnlyList<HourlyPriceRateDto> HourlyPriceRates { get; init; }
             = Array.Empty<HourlyPriceRateDto>();
        public IReadOnlyList<ProductListDto> Products { get; init; }
          = Array.Empty<ProductListDto>();
    }
}
