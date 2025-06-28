using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportSync.Data.Entities;
using SportSync.Data.Enums; // Đảm bảo bạn có các Enum này

namespace SportSync.Data
{
    // Kế thừa từ IdentityDbContext để quản lý AspNetUsers và các bảng Identity khác
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSet Properties cho tất cả các Entities của bạn ---
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<SportType> SportTypes { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<CourtComplex> CourtComplexes { get; set; }
        public DbSet<Court> Courts { get; set; }
        public DbSet<CourtComplexImage> CourtComplexImages { get; set; }
        public DbSet<CourtComplexAmenity> CourtComplexAmenities { get; set; } // Bảng nối
        public DbSet<ProductCategory> ProductCategories { get; set; } // Bảng mới
        public DbSet<Product> Products { get; set; } // Bảng mới
        public DbSet<BookingProduct> BookingProducts { get; set; } // Bảng mới
        public DbSet<HourlyPriceRate> HourlyPriceRates { get; set; } // Bảng mới
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BlockedCourtSlot> BlockedCourtSlots { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemConfiguration> SystemConfigurations { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } // Bảng mới
        public DbSet<OwnerSubscription> OwnerSubscriptions { get; set; } // Bảng mới

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đổi tên bảng AspNetUsers nếu cần (mặc định IdentityDbContext đã có)
            // builder.Entity<ApplicationUser>().ToTable("Users");

            // --- Cấu hình cho ApplicationUser (IdentityUser) ---
            builder.Entity<ApplicationUser>(entity =>
            {
                // Cấu hình mối quan hệ 1-1 giữa ApplicationUser và CourtComplex
                entity.HasOne(au => au.OwnedCourtComplex)
                    .WithOne(cc => cc.OwnerUser)
                    .HasForeignKey<CourtComplex>(cc => cc.OwnerUserId) // OwnerUserId là FK và cũng là unique key
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Restrict); // Giữ nguyên Restrict

                // Cấu hình mối quan hệ 1-nhiều với OwnerSubscriptions
                entity.HasMany(au => au.OwnerSubscriptions)
                    .WithOne(os => os.OwnerUser)
                    .HasForeignKey(os => os.OwnerUserId)
                    .OnDelete(DeleteBehavior.Restrict); // Không xóa subscriptions khi owner bị xóa
            });

            // --- Cấu hình UserProfile ---
            builder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("UserProfiles");
                entity.HasKey(e => e.UserId); // UserId là PK
                entity.Property(e => e.FullName).HasMaxLength(255);
                entity.Property(e => e.RegisteredDate).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.AccountStatusByAdmin).HasConversion<int>().HasDefaultValue(AccountStatus.Active);

                // Mối quan hệ 1-1 với ApplicationUser
                entity.HasOne(up => up.ApplicationUser)
                    .WithOne(au => au.UserProfile)
                    .HasForeignKey<UserProfile>(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Xóa profile khi user bị xóa
            });

            // --- Cấu hình SportType ---
            builder.Entity<SportType>(entity =>
            {
                entity.ToTable("SportTypes");
                entity.HasKey(e => e.SportTypeId);
                entity.Property(e => e.SportTypeId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique(); // Duy nhất
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // --- Cấu hình Amenity ---
            builder.Entity<Amenity>(entity =>
            {
                entity.ToTable("Amenities");
                entity.HasKey(e => e.AmenityId);
                entity.Property(e => e.AmenityId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Name).IsUnique(); // Duy nhất
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconCssClass).HasMaxLength(100);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // --- Cấu hình CourtComplex ---
            builder.Entity<CourtComplex>(entity =>
            {
                entity.ToTable("CourtComplexes");
                entity.HasKey(e => e.CourtComplexId);
                entity.Property(e => e.CourtComplexId).ValueGeneratedOnAdd(); // IDENTITY(1,1)

                entity.Property(e => e.OwnerUserId).IsRequired().HasMaxLength(450); // FK

                // Mối quan hệ 1-1 với ApplicationUser (được định nghĩa ở ApplicationUser)
                // builder.Entity<ApplicationUser>().HasOne(au => au.OwnedCourtComplex).WithOne(cc => cc.OwnerUser)

                entity.Property(e => e.SportTypeId).IsRequired(); // FK
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.District).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Ward).HasMaxLength(100);
                entity.Property(e => e.GoogleMapsLink).HasColumnType("nvarchar(MAX)"); // NVARCHAR(MAX)
                entity.Property(e => e.Description).HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.MainImageCloudinaryPublicId).HasMaxLength(255);
                entity.Property(e => e.MainImageCloudinaryUrl).HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.ContactPhoneNumber).HasMaxLength(20);
                entity.Property(e => e.ContactEmail).HasMaxLength(255);
                entity.Property(e => e.DefaultOpeningTime).HasColumnType("TIME");
                entity.Property(e => e.DefaultClosingTime).HasColumnType("TIME");
                entity.Property(e => e.IsActiveByOwner).HasDefaultValue(true);

                // Cấu hình thông tin ngân hàng
                entity.Property(e => e.BankCode).HasMaxLength(50);
                entity.Property(e => e.AccountNumber).HasMaxLength(50);
                entity.Property(e => e.AccountName).HasMaxLength(255);

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable, không có default value SQL

                // Mối quan hệ Nhiều-một với SportType
                entity.HasOne(cc => cc.SportType)
                    .WithMany(st => st.CourtComplexes)
                    .HasForeignKey(cc => cc.SportTypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Mối quan hệ Một-nhiều với Courts
                entity.HasMany(cc => cc.Courts)
                    .WithOne(c => c.CourtComplex)
                    .HasForeignKey(c => c.CourtComplexId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ Một-nhiều với CourtComplexImages
                entity.HasMany(cc => cc.CourtComplexImages)
                    .WithOne(cci => cci.CourtComplex)
                    .HasForeignKey(cci => cci.CourtComplexId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ Một-nhiều với Bookings
                entity.HasMany(cc => cc.Bookings)
                    .WithOne(b => b.CourtComplex)
                    .HasForeignKey(b => b.CourtComplexId)
                    .OnDelete(DeleteBehavior.Restrict); // Không xóa bookings khi complex bị xóa

                // Mối quan hệ Nhiều-nhiều với Amenities qua CourtComplexAmenities
                entity.HasMany(cc => cc.CourtComplexAmenities)
                    .WithOne(cca => cca.CourtComplex)
                    .HasForeignKey(cca => cca.CourtComplexId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ Một-nhiều với Products
                entity.HasMany(cc => cc.Products)
                    .WithOne(p => p.CourtComplex)
                    .HasForeignKey(p => p.CourtComplexId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Cấu hình Court ---
            builder.Entity<Court>(entity =>
            {
                entity.ToTable("Courts");
                entity.HasKey(e => e.CourtId);
                entity.Property(e => e.CourtId).ValueGeneratedOnAdd(); // IDENTITY(1,1)

                entity.Property(e => e.CourtComplexId).IsRequired(); // FK
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.StatusByOwner).HasConversion<int>().HasDefaultValue(CourtStatusByOwner.Available);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable

                // Mối quan hệ Một-nhiều với HourlyPriceRates
                entity.HasMany(c => c.HourlyPriceRates)
                    .WithOne(hpr => hpr.Court)
                    .HasForeignKey(hpr => hpr.CourtId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Mối quan hệ Một-nhiều với Bookings
                entity.HasMany(c => c.Bookings)
                    .WithOne(b => b.Court)
                    .HasForeignKey(b => b.CourtId)
                    .OnDelete(DeleteBehavior.Restrict); // Không xóa bookings khi court bị xóa

                // Mối quan hệ Một-nhiều với BlockedCourtSlots
                entity.HasMany(c => c.BlockedCourtSlots)
                    .WithOne(bcs => bcs.Court)
                    .HasForeignKey(bcs => bcs.CourtId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- Cấu hình CourtComplexImage ---
            builder.Entity<CourtComplexImage>(entity =>
            {
                entity.ToTable("CourtComplexImages");
                entity.HasKey(e => e.CourtComplexImageId);
                entity.Property(e => e.CourtComplexImageId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.CloudinaryPublicId).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CloudinaryUrl).IsRequired().HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.Caption).HasMaxLength(255);
                entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            });

            // --- Cấu hình CourtComplexAmenity (Bảng nối) ---
            builder.Entity<CourtComplexAmenity>(entity =>
            {
                entity.ToTable("CourtComplexAmenities");
                entity.HasKey(e => new { e.CourtComplexId, e.AmenityId }); // Composite PK
            });

            // --- Cấu hình ProductCategory ---
            builder.Entity<ProductCategory>(entity =>
            {
                entity.ToTable("ProductCategories");
                entity.HasKey(e => e.ProductCategoryId);
                entity.Property(e => e.ProductCategoryId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Name).IsUnique(); // Duy nhất
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable
            });

            // --- Cấu hình Product ---
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.CourtComplexId).IsRequired(); // FK
                entity.Property(e => e.ProductCategoryId).IsRequired(); // FK
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.ProductType).HasConversion<int>(); // Lưu enum dưới dạng int
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.StockQuantity); // Nullable
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable

                // Mối quan hệ Nhiều-một với BookingProducts
                entity.HasMany(p => p.BookingProducts)
                    .WithOne(bp => bp.Product)
                    .HasForeignKey(bp => bp.ProductId)
                    .OnDelete(DeleteBehavior.Restrict); // Không xóa booking products khi product bị xóa
            });

            // --- Cấu hình BookingProduct ---
            builder.Entity<BookingProduct>(entity =>
            {
                entity.ToTable("BookingProducts");
                entity.HasKey(e => e.BookingProductId);
                entity.Property(e => e.BookingProductId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.BookingId).IsRequired(); // FK
                entity.Property(e => e.ProductId).IsRequired(); // FK
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPriceAtTimeOfAddition).IsRequired().HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.RentalStartTime); // Nullable
                entity.Property(e => e.RentalEndTime); // Nullable
                entity.Property(e => e.Subtotal).IsRequired().HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETDATE()");
            });

            // --- Cấu hình HourlyPriceRate ---
            builder.Entity<HourlyPriceRate>(entity =>
            {
                entity.ToTable("HourlyPriceRates");
                entity.HasKey(e => e.HourlyPriceRateId);
                entity.Property(e => e.HourlyPriceRateId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.CourtId).IsRequired(); // FK
                entity.Property(e => e.DayOfWeek).HasConversion<int>(); // Lưu enum DayOfWeek dưới dạng int, nullable
                entity.Property(e => e.StartTime).IsRequired().HasColumnType("TIME");
                entity.Property(e => e.EndTime).IsRequired().HasColumnType("TIME");
                entity.Property(e => e.PricePerHour).IsRequired().HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable

                // Unique index for (CourtId, DayOfWeek, StartTime)
                entity.HasIndex(e => new { e.CourtId, e.DayOfWeek, e.StartTime }).IsUnique();
            });

            // --- Cấu hình Booking ---
            builder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(e => e.BookingId);
                entity.Property(e => e.BookingId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.BookerUserId).IsRequired().HasMaxLength(450); // FK
                entity.Property(e => e.CourtComplexId).IsRequired(); // FK
                entity.Property(e => e.CourtId).IsRequired(); // FK
                entity.Property(e => e.BookedStartTime).IsRequired(); // DateTime
                entity.Property(e => e.BookedEndTime).IsRequired(); // DateTime
                entity.Property(e => e.TotalPrice).IsRequired().HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.BookingStatus).HasConversion<int>().HasDefaultValue(BookingStatusType.Confirmed);
                entity.Property(e => e.ManualBookingCustomerName).HasMaxLength(255);
                entity.Property(e => e.ManualBookingCustomerPhone).HasMaxLength(20);
                entity.Property(e => e.NotesFromBooker).HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.NotesFromOwner).HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable
            });

            // --- Cấu hình BlockedCourtSlot ---
            builder.Entity<BlockedCourtSlot>(entity =>
            {
                entity.ToTable("BlockedCourtSlots");
                entity.HasKey(e => e.BlockedSlotId);
                entity.Property(e => e.BlockedSlotId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.CourtId).IsRequired(); // FK
                entity.Property(e => e.BlockDate).IsRequired().HasColumnType("DATE");
                entity.Property(e => e.StartTime).IsRequired().HasColumnType("TIME");
                entity.Property(e => e.EndTime).IsRequired().HasColumnType("TIME");
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.CreatedByUserId).IsRequired().HasMaxLength(450); // FK
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Unique index for (CourtId, BlockDate, StartTime)
                entity.HasIndex(e => new { e.CourtId, e.BlockDate, e.StartTime }).IsUnique();
            });

            // --- Cấu hình Notification ---
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.NotificationId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.RecipientUserId).IsRequired().HasMaxLength(450); // FK
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Message).IsRequired().HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.NotificationType).HasConversion<int>(); // Lưu enum dưới dạng int
                entity.Property(e => e.ReferenceId).HasMaxLength(100);
                entity.Property(e => e.ReferenceType).HasMaxLength(50);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.ReadAt); // Nullable
                entity.Property(e => e.DeliveryMethod).HasConversion<int>().HasDefaultValue(DeliveryMethodType.InApp);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // --- Cấu hình SystemConfiguration ---
            builder.Entity<SystemConfiguration>(entity =>
            {
                entity.ToTable("SystemConfigurations");
                entity.HasKey(e => e.ConfigurationKey); // PK là chuỗi
                entity.Property(e => e.ConfigurationKey).HasMaxLength(100);
                entity.Property(e => e.ConfigurationValue).IsRequired().HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedByAdminId).HasMaxLength(450); // FK, nullable
            });

            // --- Cấu hình SubscriptionPlan (MỚI) ---
            builder.Entity<SubscriptionPlan>(entity =>
            {
                entity.ToTable("SubscriptionPlans");
                entity.HasKey(e => e.PlanId);
                entity.Property(e => e.PlanId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique(); // Duy nhất
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.MonthlyPrice).IsRequired().HasColumnType("DECIMAL(18,2)");
                entity.Property(e => e.Features).HasColumnType("nvarchar(MAX)");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable
            });

            // --- Cấu hình OwnerSubscription (MỚI) ---
            builder.Entity<OwnerSubscription>(entity =>
            {
                entity.ToTable("OwnerSubscriptions");
                entity.HasKey(e => e.OwnerSubscriptionId);
                entity.Property(e => e.OwnerSubscriptionId).ValueGeneratedOnAdd(); // IDENTITY(1,1)
                entity.Property(e => e.OwnerUserId).IsRequired().HasMaxLength(450); // FK
                entity.Property(e => e.PlanId).IsRequired(); // FK
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate); // Nullable
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.PaymentStatus).HasConversion<int>(); // Lưu enum dưới dạng int
                entity.Property(e => e.LastPaymentDate); // Nullable
                entity.Property(e => e.NextBillingDate); // Nullable
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt); // Nullable

                // Mối quan hệ Nhiều-một với SubscriptionPlan
                entity.HasOne(os => os.Plan)
                    .WithMany(sp => sp.OwnerSubscriptions)
                    .HasForeignKey(os => os.PlanId)
                    .OnDelete(DeleteBehavior.Restrict); // Không xóa subscriptions khi plan bị xóa
            });
        }
    }
}
