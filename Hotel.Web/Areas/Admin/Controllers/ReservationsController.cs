using Hotel.Application.Interfaces;
using Hotel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.Admin.Controllers;

/// <summary>مدیریت رزروها در پنل ادمین</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ReservationsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly Hotel.Application.Interfaces.ISmsService _sms;

    public ReservationsController(IUnitOfWork uow, ISmsService sms)
    {
        _uow = uow;
        _sms = sms;
    }

    public async Task<IActionResult> Index(string? status, string? search)
    {
        var query = _uow.Reservations.Query().Include(r => r.Room).AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.GuestName.Contains(search) || r.GuestPhone.Contains(search) || r.TrackingCode.Contains(search));

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, out var st))
            query = query.Where(r => r.Status == st);

        ViewBag.Reservations = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Detail(int id)
    {
        var res = await _uow.Reservations.Query().Include(r => r.Room).FirstOrDefaultAsync(r => r.Id == id);
        if (res == null) return NotFound();
        return View(res);
    }

    /// <summary>تغییر وضعیت رزرو (AJAX)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, ReservationStatus status)
    {
        var res = await _uow.Reservations.GetByIdAsync(id);
        if (res == null) return Json(new { success = false });

        res.Status = status;
        _uow.Reservations.Update(res);
        await _uow.SaveChangesAsync();

        // ارسال پیامک لغو
        if (status == ReservationStatus.Cancelled)
            await _sms.SendCancellationAsync(res.GuestPhone, res.GuestName, res.TrackingCode);

        return Json(new { success = true });
    }

    /// <summary>تغییر وضعیت پرداخت</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePayment(int id, PaymentStatus status)
    {
        var res = await _uow.Reservations.GetByIdAsync(id);
        if (res == null) return Json(new { success = false });

        res.PaymentStatus = status;
        _uow.Reservations.Update(res);
        await _uow.SaveChangesAsync();
        return Json(new { success = true });
    }
}
