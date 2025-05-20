using SportSync.Data.Enums;

namespace SportSync.Data.Entities
{
    public class UserProfile
    {   
        public string UserId { get; set; } // Sẽ được cấu hình là PK và FK bằng Fluent API

        public string? FullName { get; set; } // Sẽ được cấu hình độ dài, nullability bằng Fluent API

        public DateTime RegisteredDate { get; set; } // Sẽ được cấu hình giá trị mặc định bằng Fluent API   

        public DateTime? LastLoginDate { get; set; } // Sẽ được cấu hình nullability bằng Fluent API

        public AccountStatus AccountStatusByAdmin { get; set; } // Sẽ được cấu hình NOT NULL, default bằng Fluent API

        // Navigation property cho mối quan hệ 1-1
        public virtual ApplicationUser ApplicationUser { get; set; }
    }
}
