namespace Hotel.Application.Interfaces;

/// <summary>اینترفیس سرویس پیامک - پیاده‌سازی Mock یا Kavenegar</summary>
public interface ISmsService
{
    /// <summary>ارسال پیامک ساده</summary>
    Task<bool> SendAsync(string phoneNumber, string message);

    /// <summary>ارسال پیامک رزرو اتاق</summary>
    Task<bool> SendReservationConfirmationAsync(string phone, string guestName, string trackingCode, DateTime checkIn);

    /// <summary>ارسال پیامک لغو رزرو</summary>
    Task<bool> SendCancellationAsync(string phone, string guestName, string trackingCode);

    /// <summary>ارسال پیامک تبلیغاتی دسته‌جمعی</summary>
    Task<int> SendBulkAsync(IEnumerable<string> phones, string message);
}
