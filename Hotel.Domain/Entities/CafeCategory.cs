namespace Hotel.Domain.Entities;

/// <summary>دسته‌بندی منوی کافی‌شاپ</summary>
public class CafeCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;      // مثلا: قهوه، دسر، نوشیدنی
    public string? IconClass { get; set; }                 // آیکون FontAwesome
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<CafeMenuItem> MenuItems { get; set; } = new List<CafeMenuItem>();
}
