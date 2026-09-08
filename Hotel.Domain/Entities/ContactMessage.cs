using System.ComponentModel.DataAnnotations;

namespace Hotel.Domain.Entities;

/// <summary>پیام‌های تماس با ما</summary>
public class ContactMessage : BaseEntity
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [Required, RegularExpression(@"^09\d{9}$"), StringLength(11)]
    public string Phone { get; set; } = string.Empty;
    [EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;
    [Required, StringLength(150)]
    public string Subject { get; set; } = string.Empty;
    [Required, StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    [StringLength(2000)]
    public string? AdminReply { get; set; }
}
