namespace Hotel.Domain.Entities;

/// <summary>تصاویر اتاق - گالری چندگانه</summary>
public class RoomImage : BaseEntity
{
    public int RoomId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; } = 0;

    public Room Room { get; set; } = null!;
}
