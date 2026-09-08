using Hotel.Domain.Enums;

namespace Hotel.Domain.Entities;

/// <summary>موجودیت اتاق هتل</summary>
public class Room : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;  // شماره اتاق
    public string Name { get; set; } = string.Empty;         // نام اتاق
    public string Description { get; set; } = string.Empty;  // توضیحات
    public RoomType RoomType { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public int Capacity { get; set; }         // ظرفیت نفر
    public decimal PricePerNight { get; set; } // قیمت هر شب
    public int Floor { get; set; }             // طبقه
    public bool HasBalcony { get; set; }       // بالکن
    public bool HasJacuzzi { get; set; }       // جکوزی
    public bool HasMinibar { get; set; }       // مینی‌بار
    public string? MainImagePath { get; set; } // تصویر اصلی

    // روابط
    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
