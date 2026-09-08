namespace Hotel.Domain.Entities;

/// <summary>تنظیمات سایت - key/value داینامیک برای همه چیز</summary>
public class SiteSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;    // کلید (مثلا: HotelName, Phone, AboutText)
    public string Value { get; set; } = string.Empty;  // مقدار
    public string? Group { get; set; }                  // گروه‌بندی (General, Contact, Social)
    public string? Description { get; set; }            // توضیح برای ادمین
}
