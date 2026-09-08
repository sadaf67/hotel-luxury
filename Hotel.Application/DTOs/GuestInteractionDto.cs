using System.ComponentModel.DataAnnotations;

namespace Hotel.Application.DTOs;

public sealed class ContactMessageDto
{
    [Required(ErrorMessage = "نام را وارد کنید"), StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل را وارد کنید"), RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست"), StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "موضوع را وارد کنید"), StringLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "متن پیام را وارد کنید"), StringLength(2000, MinimumLength = 10, ErrorMessage = "پیام باید بین ۱۰ تا ۲۰۰۰ کاراکتر باشد")]
    public string Message { get; set; } = string.Empty;
}

public sealed class ReservationTrackingDto
{
    [Required(ErrorMessage = "کد پیگیری را وارد کنید"), StringLength(40)]
    public string TrackingCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل را وارد کنید"), RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست")]
    public string Phone { get; set; } = string.Empty;
}
