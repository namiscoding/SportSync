using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportSync.Data.Entities;
using SportSync.Data;
using Google.Apis.Auth.OAuth2;
using SportSync.Business.Interfaces;
using SportSync.Business.Services;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Cors.Infrastructure;
using SportSync.Data.Interfaces;
using SportSync.Data.Repositories;
using SportSync.Business.Settings;



var builder = WebApplication.CreateBuilder(args);


// Cấu hình Session State
builder.Services.AddDistributedMemoryCache(); // Cần thiết cho session
builder.Services.AddSession(options =>

{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Thời gian OTP và session tồn tại (ví dụ: 10 phút)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo cookie session hoạt động ngay cả khi người dùng chưa chấp nhận cookie policy
});
builder.Services.AddScoped<ICourtSearchService, CourtSearchService>();
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.Configure<ESmsSettings>(builder.Configuration.GetSection("ESmsSettings"));
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
    }));

// 3. Đăng ký dịch vụ ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình các tùy chọn cho Identity (nếu cần)
    options.SignIn.RequireConfirmedAccount = false; 
    options.SignIn.RequireConfirmedPhoneNumber = true;
    options.User.RequireUniqueEmail = false;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false; // Có thể nới lỏng cho dễ nhớ
    options.Password.RequiredLength = 6;

    // Cấu hình Lockout (khóa tài khoản khi đăng nhập sai nhiều lần)
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình User
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Yêu cầu email là duy nhất
})
.AddEntityFrameworkStores<ApplicationDbContext>() // Sử dụng ApplicationDbContext để lưu trữ dữ liệu Identity
.AddDefaultTokenProviders(); // Thêm các nhà cung cấp token mặc định (ví dụ: cho reset password)

// Add services to the container.
builder.Services.AddControllersWithViews();

// (Tùy chọn) Cấu hình đường dẫn cho trang đăng nhập, từ chối truy cập, v.v.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi truy cập bị từ chối
    options.SlidingExpiration = true; // Gia hạn cookie nếu người dùng hoạt động
    // options.ExpireTimeSpan = TimeSpan.FromDays(30); // Thời gian hết hạn cookie
});

builder.Services.Configure<SportSync.Business.Settings.CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddScoped<SportSync.Business.Interfaces.IImageUploadService, SportSync.Business.Services.CloudinaryImageUploadService>();
builder.Services.AddScoped<ICourtComplexService, CourtComplexService>();

builder.Services.AddScoped<IApplicationUserService, ApplicationUserService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<CourtOwnerManagementService>();
builder.Services.AddScoped<CourtComplexManagementService>();
builder.Services.AddScoped<ICourtService, CourtService>();
builder.Services.AddScoped<CourtManagementService>();
builder.Services.AddScoped<ISportTypeService, SportTypeService>();
builder.Services.AddScoped<SportTypeManagementService>();

builder.Services.AddScoped<IBookingService, BookingService>();

builder.Services.AddScoped<ITimeSlotManagementService, TimeSlotManagementService>();
builder.Services.AddScoped<IBookingManagementService, BookingManagementService>();
builder.Services.AddScoped<ICourtOwnerDashboardService, CourtOwnerDashboardService>();
builder.Services.AddScoped<SportSync.Business.Interfaces.ITimeSlotService, SportSync.Business.Services.TimeSlotService>();
builder.Services.AddScoped<ISmsSender, ESmsSender>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Đảm bảo role "Admin", "CourtOwner", "User" đã tồn tại
        string[] roles = { "Admin", "CourtOwner", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Tạo hoặc cập nhật tài khoản Admin (đã có trong ảnh của bạn)
        var adminUser = await userManager.FindByIdAsync("abbbda5c-2b74-493c-a194-b0e276f8386");
        if (adminUser != null && !await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // Tạo dữ liệu mẫu cho User và CourtOwner
        var userData = new[]
        {
            new { Role = "User", Phone = "+84912345678", Email = "user1@example.com", FullName = "User One" },
            new { Role = "CourtOwner", Phone = "+84987654321", Email = "owner1@example.com", FullName = "Owner One" },
            new { Role = "CourtOwner", Phone = "+84812345678", Email = "owner2@example.com", FullName = "Owner Two" }
        };

        foreach (var data in userData)
        {
            var user = await userManager.FindByEmailAsync(data.Email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = data.Phone,
                    Email = data.Email,
                    PhoneNumber = data.Phone,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "User@123"); // Mật khẩu mặc định
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, data.Role);
                    context.UserProfiles.Add(new UserProfile
                    {
                        UserId = user.Id,
                        FullName = data.FullName,
                        RegisteredDate = DateTime.UtcNow,
                        AccountStatusByAdmin = 0 // Active
                    });
                }
            }
        }

        // Tạo dữ liệu mẫu cho SportTypes
        if (!context.SportTypes.Any())
        {
            context.SportTypes.AddRange(new[]
            {
                new SportType { Name = "Bóng đá", Description = "Sân bóng đá 5 người", IsActive = true },
                new SportType { Name = "Cầu lông", Description = "Sân cầu lông trong nhà", IsActive = true }
            });
        }

        // Tạo dữ liệu mẫu cho Amenities
        if (!context.Amenities.Any())
        {
            context.Amenities.AddRange(new[]
            {
                new Amenity { Name = "Đèn", Description = "Hệ thống chiếu sáng", IconCssClass = "fa-lightbulb", IsActive = true },
                new Amenity { Name = "Mái che", Description = "Che mưa nắng", IconCssClass = "fa-umbrella", IsActive = true }
            });
        }

        // Tạo dữ liệu mẫu cho CourtComplexes và Courts
        var courtOwners = await userManager.GetUsersInRoleAsync("CourtOwner");
        if (!context.CourtComplexes.Any())
        {
            var sportTypes = context.SportTypes.ToList();
            int complexId = 1;
            foreach (var owner in courtOwners)
            {
                var courtComplex = new CourtComplex
                {
                    OwnerUserId = owner.Id,
                    Name = $"Complex {complexId}",
                    Address = $"Address {complexId}, District {complexId}, City {complexId}",
                    City = $"City {complexId}",
                    District = $"District {complexId}",
                    ApprovalStatus = SportSync.Data.Enums.ApprovalStatus.PendingApproval,
                    IsActiveByOwner = true,
                    IsActiveByAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.CourtComplexes.Add(courtComplex);

                // Thêm Courts cho CourtComplex
                context.Courts.AddRange(new[]
                {
                    new Court
                    {
                        CourtComplexId = complexId,
                        SportTypeId = sportTypes[0].SportTypeId,
                        Name = $"Sân A - {complexId}",
                        DefaultSlotDurationMinutes = 60,
                        AdvanceBookingDaysLimit = 7,
                        StatusByOwner = SportSync.Data.Enums.CourtStatusByOwner.Available,
                        IsActiveByAdmin = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Court
                    {
                        CourtComplexId = complexId,
                        SportTypeId = sportTypes[1].SportTypeId,
                        Name = $"Sân B - {complexId}",
                        DefaultSlotDurationMinutes = 60,
                        AdvanceBookingDaysLimit = 7,
                        StatusByOwner = SportSync.Data.Enums.CourtStatusByOwner.Available,
                        IsActiveByAdmin = true,
                        CreatedAt = DateTime.UtcNow
                    }
                });
                complexId++;
            }
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding data: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();
app.UseSession();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "CourtOwner", "User", "StandardCourtOwner", "ProCourtOwner" };
    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Tạo role mới
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

/*
// (Tùy chọn) Phương thức ví dụ để seed một tài khoản Admin mặc định
static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    // Đảm bảo role "Admin" đã tồn tại
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Kiểm tra xem có admin user nào chưa
    var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
    if (!adminUsers.Any()) // Nếu chưa có admin nào
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@example.com", // Thay bằng email quản trị bạn muốn
            Email = "admin@example.com",    // Thay bằng email quản trị bạn muốn
            EmailConfirmed = true,          // Xác thực email luôn
            UserProfile = new UserProfile   // Khởi tạo UserProfile nếu cần thông tin ban đầu
            {
                FullName = "Administrator",
                RegisteredDate = DateTime.UtcNow,
                AccountStatusByAdmin = SportBookingWebsite.Data.Enums.AccountStatus.Active
            }
        };

        // Thay "AdminP@ssw0rd1!" bằng mật khẩu mạnh bạn muốn
        var result = await userManager.CreateAsync(adminUser, "AdminP@ssw0rd1!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // Xử lý lỗi nếu tạo user không thành công (ví dụ: log lỗi)
            // foreach (var error in result.Errors) { Console.WriteLine(error.Description); }
        }
    }
}
*/