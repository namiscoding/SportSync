using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportSync.Data.Entities;
using SportSync.Data;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using SportSync.Business.Interfaces;
using SportSync.Business.Services;


var builder = WebApplication.CreateBuilder(args);

// Cấu hình Session State
builder.Services.AddDistributedMemoryCache(); // Cần thiết cho session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10); // Thời gian OTP và session tồn tại (ví dụ: 10 phút)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Đảm bảo cookie session hoạt động ngay cả khi người dùng chưa chấp nhận cookie policy
});

// Add services to the container.
builder.Services.AddControllersWithViews();
// 1. Lấy chuỗi kết nối từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Đăng ký ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        // Chỉ định assembly chứa migrations nếu DbContext ở project khác (SportBookingWebsite.Data)
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
builder.Services.AddTransient<ISmsSender, DebugSmsSender>();

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
        await SeedRolesAsync(roleManager);
        // Bạn có thể gọi thêm các phương thức seed data khác ở đây (ví dụ: seed admin user)
        // await SeedAdminUserAsync(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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