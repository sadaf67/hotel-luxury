namespace Hotel.Infrastructure.Services;

public sealed class SmsSettings
{
    public const string SectionName = "SmsSettings";
    public string Provider { get; set; } = "Mock";
    public string? ApiKey { get; set; }
    public string? Sender { get; set; }
    public string BaseUrl { get; set; } = "https://api.kavenegar.com/v1";
}
