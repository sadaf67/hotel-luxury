namespace Hotel.Domain.Entities;

/// <summary>کلاس پایه برای همه موجودیت‌ها</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false; // حذف منطقی
}
