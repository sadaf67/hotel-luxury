using Hotel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.Admin.Controllers;

/// <summary>پنل ارسال پیامک - دستی و دسته‌جمعی</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SmsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly ISmsService _sms;

    public SmsController(IUnitOfWork uow, ISmsService sms)
    {
        _uow = uow;
        _sms = sms;
    }

    /// <summary>لاگ پیامک‌های ارسال شده</summary>
    public async Task<IActionResult> Index()
    {
        var logs = await _uow.SmsLogs.Query()
            .OrderByDescending(s => s.CreatedAt)
            .Take(100)
            .ToListAsync();
        return View(logs);
    }

    /// <summary>فرم ارسال پیامک دستی</summary>
    public IActionResult Send() => View();

    /// <summary>ارسال پیامک به یک شماره</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string phone, string message)
    {
        if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(message))
        {
            TempData["Error"] = "شماره و متن پیام الزامی است";
            return View();
        }

        var result = await _sms.SendAsync(phone, message);
        TempData[result ? "Success" : "Error"] = result ? "پیامک با موفقیت ارسال شد" : "خطا در ارسال پیامک";
        return RedirectToAction("Index");
    }

    /// <summary>ارسال پیامک دسته‌جمعی به مهمانان</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendBulk(string message, string target)
    {
        List<string> phones = new();

        if (target == "all_guests")
        {
            // همه مهمانانی که رزرو دارند
            phones = await _uow.Reservations.Query()
                .Select(r => r.GuestPhone)
                .Distinct()
                .ToListAsync();
        }

        if (!phones.Any())
        {
            TempData["Error"] = "هیچ شماره‌ای برای ارسال یافت نشد";
            return RedirectToAction("Send");
        }

        var count = await _sms.SendBulkAsync(phones, message);
        TempData["Success"] = $"پیامک به {count} نفر ارسال شد";
        return RedirectToAction("Index");
    }
}
