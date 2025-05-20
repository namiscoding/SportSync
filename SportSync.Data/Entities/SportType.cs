using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportSync.Data.Entities
{
    public class SportType
    {
        public int SportTypeId { get; set; } // Sẽ là PK, Identity bằng Fluent API
        public string Name { get; set; } // Sẽ là UNIQUE, NOT NULL, MaxLength(100) bằng Fluent API
        public string? Description { get; set; } // Sẽ là MaxLength(500), NULL bằng Fluent API
        public string? IconCloudinaryPublicId { get; set; } // Sẽ là MaxLength(255), NULL bằng Fluent API
        public string? IconCloudinaryUrl { get; set; } // Sẽ là MaxLength(MAX), NULL bằng Fluent API
        public bool IsActive { get; set; } // Sẽ là NOT NULL, Default(1) bằng Fluent API

        // Navigation property: Một loại hình thể thao có thể có nhiều sân (Courts)
        public virtual ICollection<Court> Courts { get; set; }

        public SportType()
        {
            Courts = new HashSet<Court>();
            // Khởi tạo IsActive = true nếu muốn có giá trị mặc định ở C# level,
            // nhưng tốt hơn là cấu hình default ở DB qua Fluent API.
            // IsActive = true;
        }
    }
}
