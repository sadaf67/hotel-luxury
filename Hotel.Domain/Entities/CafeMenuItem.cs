namespace Hotel.Domain.Entities;

/// <summary>آیتم منوی کافی‌شاپ</summary>
public class CafeMenuItem : BaseEntity
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;          // نام آیتم
    public string? Description { get; set; }                    // توضیحات
    public decimal Price { get; set; }                          // قیمت
    public string? ImagePath { get; set; }                     // تصویر
    public bool IsAvailable { get; set; } = true;              // موجود است
    public bool IsSpecial { get; set; } = false;               // پیشنهاد ویژه
    public int SortOrder { get; set; } = 0;
    public int CalorieCount { get; set; } = 0;                 // کالری

    public CafeCategory Category { get; set; } = null!;
}
