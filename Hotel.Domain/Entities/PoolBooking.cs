using Hotel.Domain.Enums;

namespace Hotel.Domain.Entities;

/// <summary>رزرو استخر</summary>
public class PoolBooking : BaseEntity
{
    public int PoolSessionId { get; set; }
    public DateTime BookingDate { get; set; }          // تاریخ رزرو
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public int AdultCount { get; set; }
    public int ChildCount { get; set; }
    public decimal TotalPrice { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string TrackingCode { get; set; } = string.Empty;

    public PoolSession PoolSession { get; set; } = null!;
}
