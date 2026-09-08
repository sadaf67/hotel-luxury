using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Hotel.Infrastructure.Services;

public sealed class KavenegarSmsService : ISmsService
{
    private readonly HttpClient _httpClient;
    private readonly SmsSettings _settings;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<KavenegarSmsService> _logger;

    public KavenegarSmsService(HttpClient httpClient, SmsSettings settings, IUnitOfWork uow, ILogger<KavenegarSmsService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _uow = uow;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        var log = new SmsLog { PhoneNumber = phoneNumber, Message = message, Status = SmsStatus.Pending, SentBy = "System" };
        try
        {
            var endpoint = $"{Uri.EscapeDataString(_settings.ApiKey!)}/sms/send.json";
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["receptor"] = phoneNumber,
                ["sender"] = _settings.Sender!,
                ["message"] = message
            });
            using var response = await _httpClient.PostAsync(endpoint, content);
            var payload = await response.Content.ReadFromJsonAsync<KavenegarResponse>();
            var success = response.IsSuccessStatusCode && payload?.Return?.Status == 200;
            log.Status = success ? SmsStatus.Sent : SmsStatus.Failed;
            log.SentAt = success ? DateTime.Now : null;
            log.ProviderResponse = payload?.Return?.Message ?? $"HTTP {(int)response.StatusCode}";
            return success;
        }
        catch (Exception ex)
        {
            log.Status = SmsStatus.Failed;
            log.ProviderResponse = "Provider request failed";
            _logger.LogError(ex, "Kavenegar SMS request failed");
            return false;
        }
        finally
        {
            await _uow.SmsLogs.AddAsync(log);
            await _uow.SaveChangesAsync();
        }
    }

    public Task<bool> SendReservationConfirmationAsync(string phone, string guestName, string trackingCode, DateTime checkIn) =>
        SendAsync(phone, $"مهمان گرامی {guestName}، رزرو شما با کد پیگیری {trackingCode} برای تاریخ {checkIn:yyyy/MM/dd} ثبت شد. هتل لوکس پارسیان");

    public Task<bool> SendCancellationAsync(string phone, string guestName, string trackingCode) =>
        SendAsync(phone, $"مهمان گرامی {guestName}، رزرو شما با کد {trackingCode} لغو گردید. جهت اطلاعات بیشتر با ما تماس بگیرید.");

    public async Task<int> SendBulkAsync(IEnumerable<string> phones, string message)
    {
        var count = 0;
        foreach (var phone in phones.Distinct()) if (await SendAsync(phone, message)) count++;
        return count;
    }

    private sealed class KavenegarResponse
    {
        [JsonPropertyName("return")]
        public KavenegarReturn? Return { get; set; }
    }

    private sealed class KavenegarReturn
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
