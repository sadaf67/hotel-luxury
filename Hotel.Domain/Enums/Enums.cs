namespace Hotel.Domain.Enums;

/// <summary>وضعیت رزرو</summary>
public enum ReservationStatus
{
    Pending = 0,    // در انتظار تایید
    Confirmed = 1,  // تایید شده
    CheckedIn = 2,  // وارد شده
    CheckedOut = 3, // خارج شده
    Cancelled = 4   // لغو شده
}

/// <summary>وضعیت پرداخت</summary>
public enum PaymentStatus
{
    Unpaid = 0,     // پرداخت نشده
    Paid = 1,       // پرداخت شده
    Refunded = 2    // مسترد شده
}

/// <summary>نوع اتاق</summary>
public enum RoomType
{
    Standard = 0,   // استاندارد
    Deluxe = 1,     // دلوکس
    Suite = 2,      // سوئیت
    Presidential = 3 // پرزیدنشیال
}

/// <summary>وضعیت اتاق</summary>
public enum RoomStatus
{
    Available = 0,  // آزاد
    Occupied = 1,   // اشغال
    Maintenance = 2 // در حال تعمیر
}

/// <summary>نوع تخفیف</summary>
public enum DiscountType
{
    Percentage = 0, // درصدی
    Fixed = 1       // مبلغ ثابت
}

/// <summary>وضعیت پیامک</summary>
public enum SmsStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
