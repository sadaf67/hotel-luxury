using Hotel.Domain.Enums;

namespace Hotel.Domain.Entities;

/// <summary>لاگ پیامک‌های ارسال شده</summary>
public class SmsLog : BaseEntity
{
    public string PhoneNumber { get; set; } = string.Empty;   // شماره گیرنده
    public string Message { get; set; } = string.Empty;        // متن پیام
    public SmsStatus Status { get; set; } = SmsStatus.Pending;
    public string? ProviderResponse { get; set; }              // پاسخ سرویس
    public string? SentBy { get; set; }                        // ارسال کننده (ادمین یا سیستم)
    public DateTime? SentAt { get; set; }
}
