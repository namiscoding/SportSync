using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportSync.Data.Entities; // Namespace chứa các entities của bạn
using SportSync.Data.Enums; // Namespace chứa các enums của bạn

namespace SportSync.Data
{
    // Sử dụng IdentityDbContext với ApplicationUser, IdentityRole và string cho kiểu khóa chính của User/Role.
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        // Khai báo DbSet cho tất cả các entities của bạn
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<SportType> SportTypes { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<CourtComplex> CourtComplexes { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<CourtImage> CourtImages { get; set; }
        public DbSet<CourtComplexImage> CourtComplexImages { get; set; }
        public DbSet<CourtAmenity> CourtAmenities { get; set; } // Bảng join
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookedSlot> BookedSlots { get; set; }
        public DbSet<BlockedCourtSlot> BlockedCourtSlots { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Rất quan trọng khi kế thừa từ IdentityDbContext

            // Cấu hình cho các bảng của ASP.NET Identity (tên bảng, schema, v.v.) nếu muốn
            // Ví dụ:
            // modelBuilder.Entity<ApplicationUser>().ToTable("Users", "security");
            // modelBuilder.Entity<IdentityRole>().ToTable("Roles", "security");
            // modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "security");
            // ... và các bảng khác của Identity ...
            // Nếu không có cấu hình này, EF Core sẽ sử dụng tên mặc định (AspNetUsers, AspNetRoles, ...)

            #region UserProfile Configuration
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("UserProfiles"); // Đảm bảo tên bảng đúng như thiết kế

                // Khóa chính UserId cũng là khóa ngoại trỏ tới AspNetUsers.Id
                // Mối quan hệ 1-1 giữa ApplicationUser và UserProfile
                entity.HasKey(up => up.UserId);

                entity.HasOne(up => up.ApplicationUser)
                      .WithOne(au => au.UserProfile)
                      .HasForeignKey<UserProfile>(up => up.UserId)
                      .OnDelete(DeleteBehavior.Cascade); // Khi User bị xóa, Profile cũng bị xóa

                entity.Property(up => up.FullName).HasMaxLength(255).IsRequired(false); // NULL
                entity.Property(up => up.RegisteredDate).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(up => up.LastLoginDate).IsRequired(false); // NULL
                entity.Property(up => up.AccountStatusByAdmin)
                      .IsRequired()
                      .HasDefaultValue(AccountStatus.Active) // Sử dụng enum
                      .HasConversion<int>(); // Lưu trữ dưới dạng int trong DB
            });
            #endregion

            #region ApplicationUser related configurations (cho các navigation properties đã thêm)
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Mối quan hệ 1-nhiều: Một User (Owner) có thể sở hữu nhiều CourtComplexes
                entity.HasMany(au => au.OwnedCourtComplexes)
                      .WithOne(cc => cc.OwnerUser)
                      .HasForeignKey(cc => cc.OwnerUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa User nếu đang sở hữu CourtComplex

                // Mối quan hệ 1-nhiều: Một User (Booker) có thể có nhiều Bookings
                entity.HasMany(au => au.Bookings)
                      .WithOne(b => b.BookerUser)
                      .HasForeignKey(b => b.BookerUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa User nếu có Bookings

                // Mối quan hệ 1-nhiều: Một User (Admin/Owner) có thể tạo nhiều BlockedSlots
                entity.HasMany(au => au.CreatedBlockedSlots)
                      .WithOne(bs => bs.CreatedByUser)
                      .HasForeignKey(bs => bs.CreatedByUserId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa User nếu đã tạo BlockedSlots

                // Mối quan hệ 1-nhiều: Một User có nhiều Notifications
                entity.HasMany(au => au.Notifications)
                      .WithOne(n => n.RecipientUser)
                      .HasForeignKey(n => n.RecipientUserId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Notifications của User nếu User bị xóa

                // Mối quan hệ 1-nhiều: Một User (Admin) có thể duyệt nhiều CourtComplexes
                // (ApprovedByAdminId trong CourtComplex là nullable)
                entity.HasMany(au => au.ApprovedCourtComplexesByAdmin)
                      .WithOne(cc => cc.ApprovedByAdmin)
                      .HasForeignKey(cc => cc.ApprovedByAdminId)
                      .IsRequired(false) // Khóa ngoại này là NULLABLE
                      .OnDelete(DeleteBehavior.SetNull); // Nếu Admin bị xóa, ApprovedByAdminId trong CourtComplex thành NULL

                // Mối quan hệ 1-nhiều: Một User (Admin) có thể cập nhật nhiều SystemConfigurations
                // (UpdatedByAdminId trong SystemConfiguration là nullable)
                entity.HasMany(au => au.UpdatedSystemConfigurationsByAdmin)
                      .WithOne(sc => sc.UpdatedByAdmin)
                      .HasForeignKey(sc => sc.UpdatedByAdminId)
                      .IsRequired(false) // Khóa ngoại này là NULLABLE
                      .OnDelete(DeleteBehavior.SetNull); // Nếu Admin bị xóa, UpdatedByAdminId trong SystemConfiguration thành NULL
            });
            #endregion

            #region SportType Configuration
            modelBuilder.Entity<SportType>(entity =>
            {
                entity.ToTable("SportTypes");
                entity.HasKey(st => st.SportTypeId);
                entity.Property(st => st.SportTypeId).ValueGeneratedOnAdd(); // IDENTITY(1,1)

                entity.Property(st => st.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(st => st.Name).IsUnique(); // UNIQUE constraint

                entity.Property(st => st.Description).HasMaxLength(500).IsRequired(false);
                entity.Property(st => st.IconCloudinaryPublicId).HasMaxLength(255).IsRequired(false);
                entity.Property(st => st.IconCloudinaryUrl).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(st => st.IsActive).IsRequired().HasDefaultValue(true);
            });
            #endregion

            #region Amenity Configuration
            modelBuilder.Entity<Amenity>(entity =>
            {
                entity.ToTable("Amenities");
                entity.HasKey(a => a.AmenityId);
                entity.Property(a => a.AmenityId).ValueGeneratedOnAdd();

                entity.Property(a => a.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(a => a.Name).IsUnique();

                entity.Property(a => a.Description).HasMaxLength(500).IsRequired(false);
                entity.Property(a => a.IconCssClass).HasMaxLength(100).IsRequired(false);
                entity.Property(a => a.IsActive).IsRequired().HasDefaultValue(true);
            });
            #endregion

            #region CourtComplex Configuration
            modelBuilder.Entity<CourtComplex>(entity =>
            {
                entity.ToTable("CourtComplexes");
                entity.HasKey(cc => cc.CourtComplexId);
                entity.Property(cc => cc.CourtComplexId).ValueGeneratedOnAdd();

                entity.Property(cc => cc.OwnerUserId).IsRequired(); // Khóa ngoại đã cấu hình ở ApplicationUser

                entity.Property(cc => cc.Name).IsRequired().HasMaxLength(255);
                entity.Property(cc => cc.Address).IsRequired().HasMaxLength(500);
                entity.Property(cc => cc.City).IsRequired().HasMaxLength(100);
                entity.Property(cc => cc.District).IsRequired().HasMaxLength(100);
                entity.Property(cc => cc.Ward).HasMaxLength(100).IsRequired(false);

                entity.Property(cc => cc.Latitude).HasColumnType("DECIMAL(9,6)").IsRequired(false);
                entity.Property(cc => cc.Longitude).HasColumnType("DECIMAL(9,6)").IsRequired(false);

                entity.Property(cc => cc.Description).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(cc => cc.MainImageCloudinaryPublicId).HasMaxLength(255).IsRequired(false);
                entity.Property(cc => cc.MainImageCloudinaryUrl).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(cc => cc.ContactPhoneNumber).HasMaxLength(20).IsRequired(false);
                entity.Property(cc => cc.ContactEmail).HasMaxLength(255).IsRequired(false);

                entity.Property(cc => cc.DefaultOpeningTime).HasColumnType("TIME").IsRequired(false);
                entity.Property(cc => cc.DefaultClosingTime).HasColumnType("TIME").IsRequired(false);

                entity.Property(cc => cc.ApprovalStatus)
                      .IsRequired()
                      .HasDefaultValue(ApprovalStatus.PendingApproval)
                      .HasConversion<int>();

                entity.Property(cc => cc.IsActiveByOwner).IsRequired().HasDefaultValue(true);
                entity.Property(cc => cc.IsActiveByAdmin).IsRequired().HasDefaultValue(true);
                entity.Property(cc => cc.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(cc => cc.UpdatedAt).IsRequired(false);

                entity.Property(cc => cc.ApprovedByAdminId).IsRequired(false); // Khóa ngoại đã cấu hình ở ApplicationUser
                entity.Property(cc => cc.ApprovedAt).IsRequired(false);
                entity.Property(cc => cc.RejectionReason).HasMaxLength(500).IsRequired(false);
            });
            #endregion

            #region Court Configuration
            modelBuilder.Entity<Court>(entity =>
            {
                entity.ToTable("Courts");
                entity.HasKey(c => c.CourtId);
                entity.Property(c => c.CourtId).ValueGeneratedOnAdd();

                entity.Property(c => c.CourtComplexId).IsRequired();
                entity.HasOne(c => c.CourtComplex)
                      .WithMany(cc => cc.Courts)
                      .HasForeignKey(c => c.CourtComplexId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế: ON DELETE CASCADE

                entity.Property(c => c.SportTypeId).IsRequired();
                entity.HasOne(c => c.SportType)
                      .WithMany(st => st.Courts)
                      .HasForeignKey(c => c.SportTypeId)
                      .OnDelete(DeleteBehavior.Restrict); // Ngăn xóa SportType nếu có Court đang sử dụng

                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(c => c.DefaultSlotDurationMinutes).IsRequired();
                entity.Property(c => c.AdvanceBookingDaysLimit).IsRequired().HasDefaultValue(7);

                entity.Property(c => c.OpeningTime).HasColumnType("TIME").IsRequired(false);
                entity.Property(c => c.ClosingTime).HasColumnType("TIME").IsRequired(false);

                entity.Property(c => c.StatusByOwner)
                      .IsRequired()
                      .HasDefaultValue(CourtStatusByOwner.Available)
                      .HasConversion<int>();

                entity.Property(c => c.IsActiveByAdmin).IsRequired().HasDefaultValue(true);
                entity.Property(c => c.MainImageCloudinaryPublicId).HasMaxLength(255).IsRequired(false);
                entity.Property(c => c.MainImageCloudinaryUrl).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(c => c.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.UpdatedAt).IsRequired(false);
            });
            #endregion

            #region CourtImage Configuration
            modelBuilder.Entity<CourtImage>(entity =>
            {
                entity.ToTable("CourtImages");
                entity.HasKey(ci => ci.CourtImageId);
                entity.Property(ci => ci.CourtImageId).ValueGeneratedOnAdd();

                entity.Property(ci => ci.CourtId).IsRequired();
                entity.HasOne(ci => ci.Court)
                      .WithMany(c => c.CourtImages)
                      .HasForeignKey(ci => ci.CourtId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế

                entity.Property(ci => ci.CloudinaryPublicId).IsRequired().HasMaxLength(255);
                entity.Property(ci => ci.CloudinaryUrl).IsRequired(); // NVARCHAR(MAX)
                entity.Property(ci => ci.Caption).HasMaxLength(255).IsRequired(false);
                entity.Property(ci => ci.IsPrimary).IsRequired().HasDefaultValue(false);
            });
            #endregion

            #region CourtComplexImage Configuration
            modelBuilder.Entity<CourtComplexImage>(entity =>
            {
                entity.ToTable("CourtComplexImages");
                entity.HasKey(cci => cci.CourtComplexImageId);
                entity.Property(cci => cci.CourtComplexImageId).ValueGeneratedOnAdd();

                entity.Property(cci => cci.CourtComplexId).IsRequired();
                entity.HasOne(cci => cci.CourtComplex)
                      .WithMany(cc => cc.CourtComplexImages)
                      .HasForeignKey(cci => cci.CourtComplexId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế

                entity.Property(cci => cci.CloudinaryPublicId).IsRequired().HasMaxLength(255);
                entity.Property(cci => cci.CloudinaryUrl).IsRequired(); // NVARCHAR(MAX)
                entity.Property(cci => cci.Caption).HasMaxLength(255).IsRequired(false);
                entity.Property(cci => cci.IsPrimary).IsRequired().HasDefaultValue(false);
            });
            #endregion

            #region CourtAmenity Configuration (Many-to-Many Join Table)
            modelBuilder.Entity<CourtAmenity>(entity =>
            {
                entity.ToTable("CourtAmenities");
                // Khóa chính phức hợp
                entity.HasKey(ca => new { ca.CourtId, ca.AmenityId });

                entity.HasOne(ca => ca.Court)
                      .WithMany(c => c.CourtAmenities)
                      .HasForeignKey(ca => ca.CourtId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế

                entity.HasOne(ca => ca.Amenity)
                      .WithMany(a => a.CourtAmenities)
                      .HasForeignKey(ca => ca.AmenityId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế
            });
            #endregion

            #region TimeSlot Configuration
            modelBuilder.Entity<TimeSlot>(entity =>
            {
                entity.ToTable("TimeSlots");
                entity.HasKey(ts => ts.TimeSlotId);
                entity.Property(ts => ts.TimeSlotId).ValueGeneratedOnAdd();

                entity.Property(ts => ts.CourtId).IsRequired();
                entity.HasOne(ts => ts.Court)
                      .WithMany(c => c.TimeSlots)
                      .HasForeignKey(ts => ts.CourtId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế

                entity.Property(ts => ts.StartTime).IsRequired().HasColumnType("TIME");
                entity.Property(ts => ts.EndTime).IsRequired().HasColumnType("TIME");
                entity.Property(ts => ts.Price).IsRequired().HasColumnType("DECIMAL(18,2)");

                entity.Property(ts => ts.DayOfWeek)
                      .IsRequired(false) // NULLABLE
                      .HasConversion<int?>(); // Lưu trữ DayOfWeek? dưới dạng int?

                entity.Property(ts => ts.IsActiveByOwner).IsRequired().HasDefaultValue(true);
                entity.Property(ts => ts.Notes).HasMaxLength(255).IsRequired(false);

                // UNIQUE (CourtId, StartTime, DayOfWeek)
                // Lưu ý: DayOfWeek có thể NULL. Nếu DB của bạn (SQL Server) không cho phép NULL trong UNIQUE index
                // theo cách này, bạn có thể cần một filtered index hoặc xử lý logic này ở tầng ứng dụng.
                // Với EF Core, IsUnique() áp dụng cho cả trường hợp NULL nếu DB hỗ trợ.
                // SQL Server xử lý NULL trong unique index là "NULL is not equal to NULL".
                entity.HasIndex(ts => new { ts.CourtId, ts.StartTime, ts.DayOfWeek }).IsUnique();
            });
            #endregion

            #region Booking Configuration
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(b => b.BookingId);
                entity.Property(b => b.BookingId).ValueGeneratedOnAdd(); // BIGINT IDENTITY

                entity.Property(b => b.BookerUserId).IsRequired(); // FK đã cấu hình ở ApplicationUser

                entity.Property(b => b.CourtComplexId).IsRequired();
                entity.HasOne(b => b.CourtComplex)
                      .WithMany(cc => cc.Bookings)
                      .HasForeignKey(b => b.CourtComplexId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa CourtComplex nếu có Booking

                entity.Property(b => b.CourtId).IsRequired();
                entity.HasOne(b => b.Court)
                      .WithMany(c => c.Bookings)
                      .HasForeignKey(b => b.CourtId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Court nếu có Booking

                entity.Property(b => b.BookingDate).IsRequired().HasColumnType("DATE");
                entity.Property(b => b.TotalPrice).IsRequired().HasColumnType("DECIMAL(18,2)");

                entity.Property(b => b.BookingStatus)
                      .IsRequired()
                      .HasDefaultValue(BookingStatusType.Confirmed) // Theo thảo luận mới
                      .HasConversion<int>();

                entity.Property(b => b.PaymentType) // Đổi tên enum thành PaymentMethodType cho rõ nghĩa
                      .IsRequired()
                      .HasColumnName("PaymentType") // Giữ tên cột PaymentType như trong DB design
                      .HasDefaultValue(PaymentMethodType.PayAtCourt)
                      .HasConversion<int>();

                entity.Property(b => b.PaymentStatus) // Đổi tên enum thành PaymentStatusType
                      .IsRequired()
                      .HasColumnName("PaymentStatus") // Giữ tên cột PaymentStatus
                      .HasDefaultValue(PaymentStatusType.Unpaid)
                      .HasConversion<int>();

                entity.Property(b => b.BookingSource) // Đổi tên enum thành BookingSourceType
                      .IsRequired()
                      .HasColumnName("BookingSource") // Giữ tên cột BookingSource
                      .HasDefaultValue(BookingSourceType.Website)
                      .HasConversion<int>();

                entity.Property(b => b.ManualBookingCustomerName).HasMaxLength(255).IsRequired(false);
                entity.Property(b => b.ManualBookingCustomerPhone).HasMaxLength(20).IsRequired(false);
                entity.Property(b => b.NotesFromBooker).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(b => b.NotesFromOwner).IsRequired(false); // NVARCHAR(MAX)
                entity.Property(b => b.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(b => b.UpdatedAt).IsRequired(false);
            });
            #endregion

            #region BookedSlot Configuration
            modelBuilder.Entity<BookedSlot>(entity =>
            {
                entity.ToTable("BookedSlots");
                entity.HasKey(bs => bs.BookedSlotId);
                entity.Property(bs => bs.BookedSlotId).ValueGeneratedOnAdd(); // BIGINT IDENTITY

                entity.Property(bs => bs.BookingId).IsRequired();
                entity.HasOne(bs => bs.Booking)
                      .WithMany(b => b.BookedSlots)
                      .HasForeignKey(bs => bs.BookingId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế

                entity.Property(bs => bs.TimeSlotId).IsRequired();
                entity.HasOne(bs => bs.TimeSlot)
                      .WithMany(ts => ts.BookedSlots)
                      .HasForeignKey(bs => bs.TimeSlotId)
                      .OnDelete(DeleteBehavior.Restrict); // Quan trọng: Ngăn xóa TimeSlot nếu đã được đặt

                entity.Property(bs => bs.SlotDate).IsRequired().HasColumnType("DATE");
                entity.Property(bs => bs.ActualStartTime).IsRequired().HasColumnType("TIME");
                entity.Property(bs => bs.ActualEndTime).IsRequired().HasColumnType("TIME");
                entity.Property(bs => bs.PriceAtBookingTime).IsRequired().HasColumnType("DECIMAL(18,2)");

                // UNIQUE (TimeSlotId, SlotDate, ActualStartTime)
                entity.HasIndex(bs => new { bs.TimeSlotId, bs.SlotDate, bs.ActualStartTime }).IsUnique();
            });
            #endregion

            #region BlockedCourtSlot Configuration
            modelBuilder.Entity<BlockedCourtSlot>(entity =>
            {
                entity.ToTable("BlockedCourtSlots");
                entity.HasKey(bcs => bcs.BlockedSlotId);
                entity.Property(bcs => bcs.BlockedSlotId).ValueGeneratedOnAdd();

                entity.Property(bcs => bcs.CourtId).IsRequired();
                entity.HasOne(bcs => bcs.Court)
                      .WithMany(c => c.BlockedCourtSlots)
                      .HasForeignKey(bcs => bcs.CourtId)
                      .OnDelete(DeleteBehavior.Cascade); // Theo thiết kế

                entity.Property(bcs => bcs.BlockDate).IsRequired().HasColumnType("DATE");
                entity.Property(bcs => bcs.StartTime).IsRequired().HasColumnType("TIME");
                entity.Property(bcs => bcs.EndTime).IsRequired().HasColumnType("TIME");
                entity.Property(bcs => bcs.Reason).HasMaxLength(500).IsRequired(false);
                entity.Property(bcs => bcs.CreatedByUserId).IsRequired(); // FK đã cấu hình ở ApplicationUser
                entity.Property(bcs => bcs.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");

                // UNIQUE (CourtId, BlockDate, StartTime)
                entity.HasIndex(bcs => new { bcs.CourtId, bcs.BlockDate, bcs.StartTime }).IsUnique();
            });
            #endregion

            #region Notification Configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(n => n.NotificationId);
                entity.Property(n => n.NotificationId).ValueGeneratedOnAdd(); // BIGINT IDENTITY

                entity.Property(n => n.RecipientUserId).IsRequired(); // FK đã cấu hình ở ApplicationUser

                entity.Property(n => n.Title).IsRequired().HasMaxLength(255);
                entity.Property(n => n.Message).IsRequired(); // NVARCHAR(MAX)

                entity.Property(n => n.NotificationType) // Đổi tên enum thành NotificationContentType
                      .IsRequired()
                      .HasColumnName("NotificationType") // Giữ tên cột NotificationType
                      .HasConversion<int>();

                entity.Property(n => n.ReferenceId).HasMaxLength(100).IsRequired(false);
                entity.Property(n => n.ReferenceType).HasMaxLength(50).IsRequired(false);
                entity.Property(n => n.IsRead).IsRequired().HasDefaultValue(false);
                entity.Property(n => n.ReadAt).IsRequired(false);

                entity.Property(n => n.DeliveryMethod) // Đổi tên enum thành DeliveryMethodType
                      .IsRequired()
                      .HasColumnName("DeliveryMethod") // Giữ tên cột DeliveryMethod
                      .HasDefaultValue(DeliveryMethodType.InApp)
                      .HasConversion<int>();

                entity.Property(n => n.CreatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
            });
            #endregion

            #region SystemConfiguration Configuration
            modelBuilder.Entity<SystemConfiguration>(entity =>
            {
                entity.ToTable("SystemConfigurations");
                entity.HasKey(sc => sc.ConfigurationKey); // Khóa chính

                entity.Property(sc => sc.ConfigurationKey).HasMaxLength(100);
                entity.Property(sc => sc.ConfigurationValue).IsRequired(); // NVARCHAR(MAX)
                entity.Property(sc => sc.Description).HasMaxLength(500).IsRequired(false);
                entity.Property(sc => sc.LastUpdatedAt).IsRequired().HasDefaultValueSql("GETDATE()");
                entity.Property(sc => sc.UpdatedByAdminId).IsRequired(false); // FK đã cấu hình ở ApplicationUser
            });
            #endregion
        }
    }
}