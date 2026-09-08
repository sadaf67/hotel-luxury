using Hotel.Domain.Entities;

namespace Hotel.Application.Interfaces;

/// <summary>Unit of Work - دسترسی یکپارچه به همه repository ها</summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Room> Rooms { get; }
    IRepository<RoomImage> RoomImages { get; }
    IRepository<Reservation> Reservations { get; }
    IRepository<PoolSession> PoolSessions { get; }
    IRepository<PoolBooking> PoolBookings { get; }
    IRepository<CafeCategory> CafeCategories { get; }
    IRepository<CafeMenuItem> CafeMenuItems { get; }
    IRepository<Discount> Discounts { get; }
    IRepository<SmsLog> SmsLogs { get; }
    IRepository<Slider> Sliders { get; }
    IRepository<SiteSetting> SiteSettings { get; }
    IRepository<ContactMessage> ContactMessages { get; }
    IRepository<Gallery> Galleries { get; }
    Task<int> SaveChangesAsync();
}
