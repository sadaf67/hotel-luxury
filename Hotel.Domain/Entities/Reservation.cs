using Hotel.Domain.Enums;

namespace Hotel.Domain.Entities;

/// <summary>رزرو اتاق</summary>
public class Reservation : BaseEntity
{
    public int RoomId { get; set; }
    public string GuestName { get; set; } = string.Empty;    // نام مهمان
    public string GuestPhone { get; set; } = string.Empty;   // موبایل
    public string GuestEmail { get; set; } = string.Empty;   // ایمیل
    public string NationalCode { get; set; } = string.Empty; // کد ملی
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int GuestsCount { get; set; } = 1;    // تعداد مهمانان
    public decimal TotalPrice { get; set; }       // قیمت کل
    public decimal DiscountAmount { get; set; }   // مبلغ تخفیف
    public decimal FinalPrice { get; set; }       // مبلغ نهایی
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public string? DiscountCode { get; set; }     // کد تخفیف استفاده شده
    public string? SpecialRequests { get; set; }  // درخواست‌های خاص
    public string TrackingCode { get; set; } = string.Empty; // کد پیگیری

    public Room Room { get; set; } = null!;
}
