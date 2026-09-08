using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Hotel.Infrastructure.Services;

/// <summary>سرویس Mock پیامک - برای توسعه. برای محیط واقعی با Kavenegar جایگزین کنید</summary>
public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;
    private readonly IUnitOfWork _uow;

    public MockSmsService(ILogger<MockSmsService> logger, IUnitOfWork uow)
    {
        _logger = logger;
        _uow = uow;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        // لاگ در کنسول و ذخیره در دیتابیس
        _logger.LogInformation("Development SMS mock accepted a message");

        await SaveLogAsync(phoneNumber, message);
        return true;
    }

    public async Task<bool> SendReservationConfirmationAsync(string phone, string guestName, string trackingCode, DateTime checkIn)
    {
        var message = $"مهمان گرامی {guestName}، رزرو شما با کد پیگیری {trackingCode} برای تاریخ {checkIn:yyyy/MM/dd} ثبت شد. هتل لوکس پارسیان";
        return await SendAsync(phone, message);
    }

    public async Task<bool> SendCancellationAsync(string phone, string guestName, string trackingCode)
    {
        var message = $"مهمان گرامی {guestName}، رزرو شما با کد {trackingCode} لغو گردید. جهت اطلاعات بیشتر با ما تماس بگیرید.";
        return await SendAsync(phone, message);
    }

    public async Task<int> SendBulkAsync(IEnumerable<string> phones, string message)
    {
        int count = 0;
        foreach (var phone in phones)
        {
            if (await SendAsync(phone, message)) count++;
        }
        return count;
    }

    private async Task SaveLogAsync(string phone, string message)
    {
        await _uow.SmsLogs.AddAsync(new SmsLog
        {
            PhoneNumber = phone,
            Message = message,
            Status = SmsStatus.Sent,
            SentBy = "System",
            SentAt = DateTime.Now,
            ProviderResponse = "Mock - Sent"
        });
        await _uow.SaveChangesAsync();
    }
}
