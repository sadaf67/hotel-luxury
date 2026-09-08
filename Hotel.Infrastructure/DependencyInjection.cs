using Hotel.Application.Interfaces;
using Hotel.Application.Services;
using Hotel.Infrastructure.Data;
using Hotel.Infrastructure.Repositories;
using Hotel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hotel.Infrastructure;

/// <summary>ثبت سرویس‌های Infrastructure در DI Container</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        // ثبت DbContext با SQL Server
        services.AddDbContext<HotelDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        // ثبت Unit of Work و Repository
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var smsSettings = new SmsSettings
        {
            Provider = config[$"{SmsSettings.SectionName}:Provider"] ?? "Mock",
            ApiKey = config[$"{SmsSettings.SectionName}:ApiKey"],
            Sender = config[$"{SmsSettings.SectionName}:Sender"],
            BaseUrl = config[$"{SmsSettings.SectionName}:BaseUrl"] ?? "https://api.kavenegar.com/v1"
        };
        services.AddSingleton(smsSettings);
        var smsProvider = config[$"{SmsSettings.SectionName}:Provider"];
        if (string.Equals(smsProvider, "Kavenegar", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton(new HttpClient
            {
                BaseAddress = new Uri(smsSettings.BaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(20)
            });
            services.AddScoped<ISmsService, KavenegarSmsService>();
        }
        else
        {
            services.AddScoped<ISmsService, MockSmsService>();
        }

        // ثبت سرویس‌های Application
        services.AddScoped<ReservationService>();

        return services;
    }
}
