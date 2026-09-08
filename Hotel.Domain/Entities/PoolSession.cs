namespace Hotel.Domain.Entities;

/// <summary>جلسه‌های استخر - برنامه زمانی و قیمت‌گذاری</summary>
public class PoolSession : BaseEntity
{
    public string Title { get; set; } = string.Empty;        // عنوان (صبح، عصر، شب)
    public TimeSpan StartTime { get; set; }                   // ساعت شروع
    public TimeSpan EndTime { get; set; }                     // ساعت پایان
    public decimal AdultPrice { get; set; }                   // قیمت بزرگسال
    public decimal ChildPrice { get; set; }                   // قیمت کودک
    public int MaxCapacity { get; set; }                      // حداکثر ظرفیت
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public string? DaysOfWeek { get; set; } // مثلا: "1,2,3,4,5" برای روزهای هفته

    public ICollection<PoolBooking> Bookings { get; set; } = new List<PoolBooking>();
}
