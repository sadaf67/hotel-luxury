using Hotel.Infrastructure;
using Hotel.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Threading.RateLimiting;
using Hotel.Web.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
ValidateProductionConfiguration(builder.Configuration, builder.Environment);

// افزودن سرویس‌های MVC
builder.Services.AddControllersWithViews();

// افزودن Identity برای ادمین
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<HotelDbContext>()
.AddDefaultTokenProviders();

// تنظیم مسیر لاگین
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Account/Login";
    options.AccessDeniedPath = "/Admin/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// افزودن سرویس‌های Infrastructure (DbContext، Repository، SMS، ...)
builder.Services.AddInfrastructure(builder.Configuration);

// Session برای فلش‌پیام‌ها
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0 }));
    options.AddPolicy("public-write", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 12, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");

var dataProtectionPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionPath))
{
    Directory.CreateDirectory(dataProtectionPath);
    builder.Services.AddDataProtection()
        .SetApplicationName("HotelLuxury")
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath));
}

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var proxy in builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [])
        if (IPAddress.TryParse(proxy, out var ip)) options.KnownProxies.Add(ip);
});

var app = builder.Build();

// Migration و داده نمایشی فقط در توسعه؛ در production مهاجرت باید مرحله جداگانه استقرار باشد.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    if (app.Environment.IsDevelopment())
    {
        await db.Database.MigrateAsync();
        await SeedContentAsync(db);
    }
    else if (!await db.Database.CanConnectAsync())
    {
        throw new InvalidOperationException("Cannot connect to the production database.");
    }

    // ایجاد ادمین پیش‌فرض اگر وجود نداشت
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedAdminAsync(userManager, roleManager, builder.Configuration);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (builder.Configuration.GetValue<bool>("ReverseProxy:Enabled")) app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    context.Response.Headers.ContentSecurityPolicy = "default-src 'self'; img-src 'self' data: https://images.unsplash.com; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; script-src 'self' 'unsafe-inline'; connect-src 'self'";
    await next();
});
app.UseStaticFiles();
var externalUploadsPath = builder.Configuration["Storage:UploadsPath"];
if (!string.IsNullOrWhiteSpace(externalUploadsPath))
{
    Directory.CreateDirectory(externalUploadsPath);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(externalUploadsPath),
        RequestPath = "/uploads"
    });
}
app.UseRouting();
app.UseRateLimiter();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// مسیر ادمین
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

// مسیر پیش‌فرض سایت
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHealthChecks("/health");

app.Run();

/// <summary>ایجاد کاربر ادمین اولیه</summary>
static async Task SeedAdminAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration config)
{
    const string adminRole = "Admin";
    var adminEmail = config["AdminSeed:Email"];
    var adminPass = config["AdminSeed:Password"];

    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPass))
        return;

    if (!await roleManager.RoleExistsAsync(adminRole))
        await roleManager.CreateAsync(new IdentityRole(adminRole));

    var admin = await userManager.FindByEmailAsync(adminEmail);
    if (admin == null)
    {
        admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var createResult = await userManager.CreateAsync(admin, adminPass);
        if (!createResult.Succeeded)
            throw new InvalidOperationException("Admin creation failed: " + string.Join(", ", createResult.Errors.Select(e => e.Description)));
        var roleResult = await userManager.AddToRoleAsync(admin, adminRole);
        if (!roleResult.Succeeded)
            throw new InvalidOperationException("Adding the admin role failed.");
    }
}

static void ValidateProductionConfiguration(IConfiguration config, IWebHostEnvironment environment)
{
    if (environment.IsDevelopment()) return;

    var errors = new List<string>();
    var connectionString = config.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("localdb", StringComparison.OrdinalIgnoreCase))
        errors.Add("ConnectionStrings:DefaultConnection must target the hosting SQL Server.");
    if (string.IsNullOrWhiteSpace(config["AllowedHosts"]) || config["AllowedHosts"] == "*" || config["AllowedHosts"]!.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        errors.Add("AllowedHosts must contain the production domain.");
    if (string.IsNullOrWhiteSpace(config["DataProtection:KeysPath"]))
        errors.Add("DataProtection:KeysPath must point to a persistent writable directory.");
    if (string.IsNullOrWhiteSpace(config["Storage:UploadsPath"]))
        errors.Add("Storage:UploadsPath must point to a persistent writable directory.");

    if (!string.Equals(config["SmsSettings:Provider"], "Kavenegar", StringComparison.OrdinalIgnoreCase))
        errors.Add("SmsSettings:Provider must be Kavenegar in production.");
    if (string.IsNullOrWhiteSpace(config["SmsSettings:ApiKey"]) || string.IsNullOrWhiteSpace(config["SmsSettings:Sender"]))
        errors.Add("SmsSettings:ApiKey and SmsSettings:Sender are required in production.");

    var adminEmail = config["AdminSeed:Email"];
    var adminPassword = config["AdminSeed:Password"];
    if (string.IsNullOrWhiteSpace(adminEmail) != string.IsNullOrWhiteSpace(adminPassword))
        errors.Add("AdminSeed:Email and AdminSeed:Password must be supplied together.");

    if (errors.Count > 0)
        throw new InvalidOperationException("Invalid production configuration: " + string.Join(" | ", errors));
}

static async Task SeedContentAsync(HotelDbContext db)
{
    if (!await db.Rooms.AnyAsync())
    {
        db.Rooms.AddRange(
            new Hotel.Domain.Entities.Room { RoomNumber="401", Name="سوئیت رویال آتریوم", Description="سوئیتی وسیع با نشیمن مستقل، نور طبیعی و جزئیات گرم برای اقامت‌های خاص و طولانی.", RoomType=Hotel.Domain.Enums.RoomType.Suite, Capacity=3, Floor=4, PricePerNight=12800000, HasBalcony=true, HasJacuzzi=true, HasMinibar=true, MainImagePath="https://images.unsplash.com/photo-1611892440504-42a792e24d32?auto=format&fit=crop&w=1400&q=85" },
            new Hotel.Domain.Entities.Room { RoomNumber="607", Name="اتاق دلوکس پانوراما", Description="چشم‌انداز باز شهر، تخت کینگ و فضای کار مینیمال برای سفرهای تفریحی یا کاری.", RoomType=Hotel.Domain.Enums.RoomType.Deluxe, Capacity=2, Floor=6, PricePerNight=7900000, HasBalcony=true, HasMinibar=true, MainImagePath="https://images.unsplash.com/photo-1590490360182-c33d57733427?auto=format&fit=crop&w=1400&q=85" },
            new Hotel.Domain.Entities.Room { RoomNumber="203", Name="اتاق کلاسیک آرام", Description="انتخابی جمع‌وجور و خوش‌ساخت با تمام امکانات ضروری و عایق صوتی کامل.", RoomType=Hotel.Domain.Enums.RoomType.Standard, Capacity=2, Floor=2, PricePerNight=4950000, HasMinibar=true, MainImagePath="https://images.unsplash.com/photo-1566665797739-1674de7a421a?auto=format&fit=crop&w=1400&q=85" }
        );
    }
    if (!await db.Sliders.AnyAsync()) db.Sliders.Add(new Hotel.Domain.Entities.Slider { Title="اقامت، به سبک شما", Subtitle="آرامش معاصر در قلب شهر؛ با میزبانی دقیق و شخصی‌سازی‌شده", ImagePath="https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=2000&q=88", ButtonText="انتخاب اتاق", ButtonUrl="/Home/Rooms", IsActive=true });
    if (!await db.PoolSessions.AnyAsync()) db.PoolSessions.AddRange(
        new Hotel.Domain.Entities.PoolSession { Title="صبح آرام", StartTime=new TimeSpan(7,0,0), EndTime=new TimeSpan(10,0,0), AdultPrice=450000, ChildPrice=250000, MaxCapacity=24, Description="سانس آرام صبحگاهی همراه با سونا و جکوزی" },
        new Hotel.Domain.Entities.PoolSession { Title="عصر خانواده", StartTime=new TimeSpan(15,0,0), EndTime=new TimeSpan(18,0,0), AdultPrice=550000, ChildPrice=300000, MaxCapacity=32, Description="فضای مناسب خانواده و کودکان" }
    );
    if (!await db.CafeCategories.AnyAsync())
    {
        var coffee = new Hotel.Domain.Entities.CafeCategory { Name="قهوه‌های تخصصی", SortOrder=1 };
        coffee.MenuItems.Add(new Hotel.Domain.Entities.CafeMenuItem { Name="لاته زعفران", Description="اسپرسو، شیر و زعفران ایرانی", Price=285000, IsSpecial=true });
        coffee.MenuItems.Add(new Hotel.Domain.Entities.CafeMenuItem { Name="آمریکانو سینگل اوریجین", Description="رست متوسط با نت شکلات تلخ", Price=210000 });
        var dessert = new Hotel.Domain.Entities.CafeCategory { Name="دسرهای امضادار", SortOrder=2 };
        dessert.MenuItems.Add(new Hotel.Domain.Entities.CafeMenuItem { Name="چیزکیک پسته", Description="پسته کرمان و پنیر خامه‌ای", Price=340000, IsSpecial=true });
        db.CafeCategories.AddRange(coffee, dessert);
    }
    if (!await db.Galleries.AnyAsync()) db.Galleries.AddRange(
        new Hotel.Domain.Entities.Gallery { Title="لابی آتریوم", Category="هتل", ImagePath="https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=85" },
        new Hotel.Domain.Entities.Gallery { Title="اقامت رویال", Category="اتاق", ImagePath="https://images.unsplash.com/photo-1591088398332-8a7791972843?auto=format&fit=crop&w=1200&q=85" },
        new Hotel.Domain.Entities.Gallery { Title="استخر آرامش", Category="استخر", ImagePath="https://images.unsplash.com/photo-1576013551627-0cc20b96c2a7?auto=format&fit=crop&w=1200&q=85" }
    );
    await db.SaveChangesAsync();
}
