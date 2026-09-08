namespace Hotel.Domain.Entities;

/// <summary>گالری تصاویر هتل - قابل مدیریت توسط ادمین</summary>
public class Gallery : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string? Category { get; set; }   // هتل، استخر، کافه، رستوران
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
