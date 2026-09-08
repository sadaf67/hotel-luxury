using Hotel.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Infrastructure.Data;

/// <summary>DbContext اصلی با Identity برای ادمین</summary>
public class HotelDbContext : IdentityDbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomImage> RoomImages { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<PoolSession> PoolSessions { get; set; }
    public DbSet<PoolBooking> PoolBookings { get; set; }
    public DbSet<CafeCategory> CafeCategories { get; set; }
    public DbSet<CafeMenuItem> CafeMenuItems { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<SmsLog> SmsLogs { get; set; }
    public DbSet<Slider> Sliders { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }
    public DbSet<Gallery> Galleries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // فیلتر حذف منطقی برای همه موجودیت‌ها
        builder.Entity<Room>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<RoomImage>().HasQueryFilter(e => !e.IsDeleted && !e.Room.IsDeleted);
        builder.Entity<Reservation>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PoolSession>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<PoolBooking>().HasQueryFilter(e => !e.IsDeleted && !e.PoolSession.IsDeleted);
        builder.Entity<CafeMenuItem>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Discount>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Slider>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Gallery>().HasQueryFilter(e => !e.IsDeleted);

        // تنظیم دقت اعشار برای قیمت‌ها
        builder.Entity<Room>().Property(r => r.PricePerNight).HasColumnType("decimal(18,0)");
        builder.Entity<Reservation>().Property(r => r.TotalPrice).HasColumnType("decimal(18,0)");
        builder.Entity<Reservation>().Property(r => r.FinalPrice).HasColumnType("decimal(18,0)");
        builder.Entity<Reservation>().Property(r => r.DiscountAmount).HasColumnType("decimal(18,0)");
        builder.Entity<Discount>().Property(d => d.Value).HasColumnType("decimal(18,2)");
        builder.Entity<CafeMenuItem>().Property(m => m.Price).HasColumnType("decimal(18,0)");
        builder.Entity<PoolSession>().Property(p => p.AdultPrice).HasColumnType("decimal(18,0)");
        builder.Entity<PoolSession>().Property(p => p.ChildPrice).HasColumnType("decimal(18,0)");
        builder.Entity<PoolBooking>().Property(p => p.TotalPrice).HasColumnType("decimal(18,0)");
        builder.Entity<Discount>().Property(d => d.MinimumAmount).HasColumnType("decimal(18,0)");
        builder.Entity<Discount>().Property(d => d.MaximumDiscount).HasColumnType("decimal(18,0)");

        // داده‌های اولیه تنظیمات سایت
        builder.Entity<SiteSetting>().HasData(
            new SiteSetting { Id = 1, Key = "HotelName", Value = "هتل لوکس پارسیان", Group = "General", Description = "نام هتل" },
            new SiteSetting { Id = 2, Key = "HotelSlogan", Value = "تجربه‌ای بی‌نظیر از اقامت لوکس", Group = "General", Description = "شعار هتل" },
            new SiteSetting { Id = 3, Key = "Phone", Value = "021-12345678", Group = "Contact", Description = "شماره تلفن" },
            new SiteSetting { Id = 4, Key = "Mobile", Value = "0912-000-0000", Group = "Contact", Description = "موبایل" },
            new SiteSetting { Id = 5, Key = "Email", Value = "info@hotel.com", Group = "Contact", Description = "ایمیل" },
            new SiteSetting { Id = 6, Key = "Address", Value = "تهران، خیابان ولیعصر", Group = "Contact", Description = "آدرس" },
            new SiteSetting { Id = 7, Key = "Instagram", Value = "#", Group = "Social", Description = "لینک اینستاگرام" },
            new SiteSetting { Id = 8, Key = "Telegram", Value = "#", Group = "Social", Description = "لینک تلگرام" },
            new SiteSetting { Id = 9, Key = "AboutText", Value = "هتل لوکس پارسیان با بیش از ۲۰ سال سابقه درخشان...", Group = "About", Description = "متن درباره ما" },
            new SiteSetting { Id = 10, Key = "WorkingHours", Value = "۲۴ ساعته", Group = "General", Description = "ساعت کاری" }
        );
    }
}
