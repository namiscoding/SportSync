using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Business.Dtos
{
    public sealed class CourtSearchRequest
    {
        /* 1. Loại sân (bóng đá, cầu lông, ...) – null ⇒ tất cả loại */
        public int? SportTypeId { get; init; }

        /* 2a. Vị trí địa lý theo city/district (nếu dùng dropdown)   */
        public string? City { get; init; }
        public string? District { get; init; }

        /* 2b. Vị trí GPS + bán kính (nếu front-end Mapbox gửi lat/lng) */
        public double? UserLat { get; init; }   // vĩ độ   (null ⇒ không lọc GPS)
        public double? UserLng { get; init; }   // kinh độ
        public double? RadiusKm { get; init; }   // bán kính Km

        /* 3. Ngày đặt (bắt buộc) */
        public required DateOnly Date { get; init; }

        /* 4. Khoảng giờ – để null nghĩa là cả ngày */
        public TimeOnly? FromTime { get; init; }
        public TimeOnly? ToTime { get; init; }
    }
    /// <summary>
    /// Slot còn trống của một sân trong ngày truy vấn.
    /// </summary>
    public sealed class TimeSlotDto
    {
        public int TimeSlotId { get; init; }
        public TimeOnly Start { get; init; }
        public TimeOnly End { get; init; }
        public decimal Price { get; init; }
    }

    /// <summary>
    /// Thông tin một sân + các slot trống & tiện ích.
    /// </summary>
    public sealed class CourtWithSlotsDto
    {
        public int CourtId { get; init; }
        public string Name { get; init; } = null!;
        public string SportTypeName { get; init; } = null!;
        public string? ImageUrl { get; init; }

        /// <summary>Danh sách tiện ích (chỉ tên; nếu cần thêm IconUrl thì mở rộng).</summary>
        public IEnumerable<string> Amenities { get; init; } = Enumerable.Empty<string>();

        /// <summary>Giá rẻ nhất trong ngày – dùng để sort hoặc hiển thị badge.</summary>
        public decimal MinPrice => AvailableSlots.Any()
                                    ? AvailableSlots.Min(s => s.Price)
                                    : 0;

        public IEnumerable<TimeSlotDto> AvailableSlots { get; init; } = [];
    }

    /// <summary>
    /// Kết quả tìm kiếm cho 1 hệ thống sân (CourtComplex).
    /// </summary>
    public sealed class CourtComplexResultDto
    {
        public int ComplexId { get; init; }
        public string Name { get; init; } = null!;
        public string Address { get; init; } = null!;
        public string? ThumbnailUrl { get; init; }

        /// <summary>Khoảng cách đến vị trí người dùng (km). Null nếu không lọc theo GPS.</summary>
        public double? DistanceKm { get; init; }

        /// <summary>Danh sách sân con có ít nhất 1 slot trống.</summary>
        public IEnumerable<CourtWithSlotsDto> Courts { get; init; } = [];
    }

}
