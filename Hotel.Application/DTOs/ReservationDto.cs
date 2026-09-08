using System.ComponentModel.DataAnnotations;
using Hotel.Domain.Enums;

namespace Hotel.Application.DTOs;

/// <summary>DTO برای ایجاد رزرو جدید</summary>
public class CreateReservationDto
{
    [Required(ErrorMessage = "اتاق را انتخاب کنید")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "نام را وارد کنید"), StringLength(100, MinimumLength = 2)]
    public string GuestName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل را وارد کنید")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
    public string GuestPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست"), StringLength(150)]
    public string GuestEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "کد ملی را وارد کنید")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "کد ملی باید 10 رقم باشد")]
    public string NationalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاریخ ورود را انتخاب کنید")]
    public DateTime CheckIn { get; set; }

    [Required(ErrorMessage = "تاریخ خروج را انتخاب کنید")]
    public DateTime CheckOut { get; set; }

    [Range(1, 10, ErrorMessage = "تعداد مهمانان باید بین 1 تا 10 باشد")]
    public int GuestsCount { get; set; } = 1;

    [StringLength(50)]
    public string? DiscountCode { get; set; }
    [StringLength(1000)]
    public string? SpecialRequests { get; set; }
}

/// <summary>DTO نمایش رزرو در ادمین</summary>
public class ReservationListDto
{
    public int Id { get; set; }
    public string TrackingCode { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal FinalPrice { get; set; }
    public ReservationStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>DTO آمار داشبورد ادمین</summary>
public class DashboardStatsDto
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ActiveGuests { get; set; }           // چک‌این شده
    public decimal TotalRevenue { get; set; }        // کل درآمد ماه جاری
    public decimal MonthlyRevenue { get; set; }
    public int AvailableRooms { get; set; }
    public int TotalRooms { get; set; }
    public int UnreadMessages { get; set; }
    public int TodayCheckIns { get; set; }
    public int TodayCheckOuts { get; set; }
    public List<MonthlyRevenueDto> RevenueChart { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
