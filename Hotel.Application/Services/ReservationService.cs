using Hotel.Application.DTOs;
using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Hotel.Application.Services;

/// <summary>سرویس مدیریت رزرو اتاق</summary>
public class ReservationService
{
    private readonly IUnitOfWork _uow;
    private readonly ISmsService _sms;

    public ReservationService(IUnitOfWork uow, ISmsService sms)
    {
        _uow = uow;
        _sms = sms;
    }

    /// <summary>ایجاد رزرو جدید با محاسبه قیمت و تخفیف</summary>
    public async Task<(bool Success, string Message, string? TrackingCode)> CreateReservationAsync(CreateReservationDto dto)
    {
        // بررسی در دسترس بودن اتاق در بازه زمانی
        var isAvailable = await CheckAvailabilityAsync(dto.RoomId, dto.CheckIn, dto.CheckOut);
        if (!isAvailable)
            return (false, "اتاق در این بازه زمانی رزرو شده است", null);

        var room = await _uow.Rooms.GetByIdAsync(dto.RoomId);
        if (room == null) return (false, "اتاق یافت نشد", null);

        var nights = (dto.CheckOut - dto.CheckIn).Days;
        if (nights <= 0) return (false, "تاریخ خروج باید بعد از تاریخ ورود باشد", null);

        var totalPrice = room.PricePerNight * nights;
        decimal discountAmount = 0;

        // اعمال کد تخفیف
        if (!string.IsNullOrWhiteSpace(dto.DiscountCode))
        {
            var (valid, discount, msg) = await ValidateDiscountAsync(dto.DiscountCode, totalPrice);
            if (!valid) return (false, msg, null);
            discountAmount = discount;
        }

        var trackingCode = GenerateTrackingCode();
        var reservation = new Reservation
        {
            RoomId = dto.RoomId,
            GuestName = dto.GuestName,
            GuestPhone = dto.GuestPhone,
            GuestEmail = dto.GuestEmail,
            NationalCode = dto.NationalCode,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            GuestsCount = dto.GuestsCount,
            TotalPrice = totalPrice,
            DiscountAmount = discountAmount,
            FinalPrice = totalPrice - discountAmount,
            DiscountCode = dto.DiscountCode,
            SpecialRequests = dto.SpecialRequests,
            TrackingCode = trackingCode,
            Status = ReservationStatus.Pending
        };

        await _uow.Reservations.AddAsync(reservation);

        // اگر کد تخفیف استفاده شد، شمارنده را افزایش بده
        if (!string.IsNullOrWhiteSpace(dto.DiscountCode))
        {
            var disc = await _uow.Discounts.FirstOrDefaultAsync(d => d.Code == dto.DiscountCode);
            if (disc != null) { disc.UsedCount++; _uow.Discounts.Update(disc); }
        }

        await _uow.SaveChangesAsync();

        // ارسال پیامک تایید
        await _sms.SendReservationConfirmationAsync(dto.GuestPhone, dto.GuestName, trackingCode, dto.CheckIn);

        return (true, "رزرو با موفقیت ثبت شد", trackingCode);
    }

    /// <summary>بررسی تداخل رزرو در بازه زمانی</summary>
    public async Task<bool> CheckAvailabilityAsync(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var conflicts = await _uow.Reservations
            .Query()
            .Where(r => r.RoomId == roomId
                && r.Status != ReservationStatus.Cancelled
                && r.CheckIn < checkOut
                && r.CheckOut > checkIn)
            .AnyAsync();
        return !conflicts;
    }

    /// <summary>اعتبارسنجی کد تخفیف و محاسبه مبلغ</summary>
    public async Task<(bool Valid, decimal Amount, string Message)> ValidateDiscountAsync(string code, decimal totalAmount)
    {
        var discount = await _uow.Discounts.FirstOrDefaultAsync(d =>
            d.Code == code && d.IsActive && !d.IsDeleted);

        if (discount == null) return (false, 0, "کد تخفیف معتبر نیست");
        if (discount.EndDate.HasValue && discount.EndDate < DateTime.Now)
            return (false, 0, "کد تخفیف منقضی شده");
        if (discount.MaxUsageCount.HasValue && discount.UsedCount >= discount.MaxUsageCount)
            return (false, 0, "ظرفیت استفاده از این کد تمام شده");
        if (discount.MinimumAmount.HasValue && totalAmount < discount.MinimumAmount)
            return (false, 0, $"حداقل مبلغ خرید برای این کد {discount.MinimumAmount:N0} تومان است");

        decimal amount = discount.Type == DiscountType.Percentage
            ? totalAmount * discount.Value / 100
            : discount.Value;

        if (discount.MaximumDiscount.HasValue && amount > discount.MaximumDiscount)
            amount = discount.MaximumDiscount.Value;

        return (true, amount, "کد تخفیف اعمال شد");
    }

    /// <summary>آمار داشبورد ادمین</summary>
    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var allReservations = _uow.Reservations.Query().Where(r => !r.IsDeleted);
        var rooms = await _uow.Rooms.GetAllAsync();
        var activeRooms = rooms.Where(r => !r.IsDeleted).ToList();

        // آمار رزروهای ماه جاری
        var monthRevenue = await allReservations
            .Where(r => r.CreatedAt >= startOfMonth && r.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(r => r.FinalPrice);

        // نمودار 6 ماه اخیر
        var revenueChart = new List<MonthlyRevenueDto>();
        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var start = new DateTime(month.Year, month.Month, 1);
            var end = start.AddMonths(1);
            var rev = await allReservations
                .Where(r => r.CreatedAt >= start && r.CreatedAt < end && r.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(r => r.FinalPrice);
            revenueChart.Add(new MonthlyRevenueDto { Month = month.ToString("MMM yy"), Amount = rev });
        }

        return new DashboardStatsDto
        {
            TotalReservations = await allReservations.CountAsync(),
            PendingReservations = await allReservations.CountAsync(r => r.Status == ReservationStatus.Pending),
            ActiveGuests = await allReservations.CountAsync(r => r.Status == ReservationStatus.CheckedIn),
            MonthlyRevenue = monthRevenue,
            TotalRevenue = await allReservations.Where(r => r.PaymentStatus == PaymentStatus.Paid).SumAsync(r => r.FinalPrice),
            AvailableRooms = activeRooms.Count(r => r.Status == RoomStatus.Available),
            TotalRooms = activeRooms.Count,
            UnreadMessages = await _uow.ContactMessages.Query().CountAsync(m => !m.IsRead),
            TodayCheckIns = await allReservations.CountAsync(r => r.CheckIn.Date == now.Date),
            TodayCheckOuts = await allReservations.CountAsync(r => r.CheckOut.Date == now.Date),
            RevenueChart = revenueChart
        };
    }

    private static string GenerateTrackingCode() =>
        $"HTL{DateTime.UtcNow:yyyyMMdd}{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";
}
