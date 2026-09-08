namespace Hotel.Domain.Entities;

/// <summary>اسلایدر صفحه اصلی - کاملاً داینامیک</summary>
public class Slider : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
    public int SortOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
