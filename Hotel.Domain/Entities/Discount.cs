using Hotel.Domain.Enums;

namespace Hotel.Domain.Entities;

/// <summary>کد تخفیف</summary>
public class Discount : BaseEntity
{
    public string Code { get; set; } = string.Empty;         // کد تخفیف
    public string Title { get; set; } = string.Empty;         // عنوان
    public DiscountType Type { get; set; }                    // درصدی یا مبلغ ثابت
    public decimal Value { get; set; }                        // مقدار تخفیف
    public decimal? MinimumAmount { get; set; }               // حداقل مبلغ خرید
    public decimal? MaximumDiscount { get; set; }             // حداکثر مبلغ تخفیف
    public DateTime? StartDate { get; set; }                  // تاریخ شروع
    public DateTime? EndDate { get; set; }                    // تاریخ انقضا
    public int? MaxUsageCount { get; set; }                   // حداکثر استفاده
    public int UsedCount { get; set; } = 0;                   // تعداد استفاده شده
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
}
