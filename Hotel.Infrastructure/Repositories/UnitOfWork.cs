using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Infrastructure.Data;

namespace Hotel.Infrastructure.Repositories;

/// <summary>پیاده‌سازی Unit of Work</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly HotelDbContext _context;

    public UnitOfWork(HotelDbContext context)
    {
        _context = context;
        Rooms = new Repository<Room>(context);
        RoomImages = new Repository<RoomImage>(context);
        Reservations = new Repository<Reservation>(context);
        PoolSessions = new Repository<PoolSession>(context);
        PoolBookings = new Repository<PoolBooking>(context);
        CafeCategories = new Repository<CafeCategory>(context);
        CafeMenuItems = new Repository<CafeMenuItem>(context);
        Discounts = new Repository<Discount>(context);
        SmsLogs = new Repository<SmsLog>(context);
        Sliders = new Repository<Slider>(context);
        SiteSettings = new Repository<SiteSetting>(context);
        ContactMessages = new Repository<ContactMessage>(context);
        Galleries = new Repository<Gallery>(context);
    }

    public IRepository<Room> Rooms { get; }
    public IRepository<RoomImage> RoomImages { get; }
    public IRepository<Reservation> Reservations { get; }
    public IRepository<PoolSession> PoolSessions { get; }
    public IRepository<PoolBooking> PoolBookings { get; }
    public IRepository<CafeCategory> CafeCategories { get; }
    public IRepository<CafeMenuItem> CafeMenuItems { get; }
    public IRepository<Discount> Discounts { get; }
    public IRepository<SmsLog> SmsLogs { get; }
    public IRepository<Slider> Sliders { get; }
    public IRepository<SiteSetting> SiteSettings { get; }
    public IRepository<ContactMessage> ContactMessages { get; }
    public IRepository<Gallery> Galleries { get; }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose() => _context.Dispose();
}
