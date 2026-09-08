using Hotel.Application.DTOs;
using Hotel.Application.Interfaces;
using Hotel.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace Hotel.Web.Controllers;

/// <summary>کنترلر رزرو اتاق و استخر</summary>
public class ReservationController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly ReservationService _reservationService;

    public ReservationController(IUnitOfWork uow, ReservationService reservationService)
    {
        _uow = uow;
        _reservationService = reservationService;
    }

    /// <summary>فرم رزرو اتاق</summary>
    public async Task<IActionResult> Book(int roomId, DateTime? checkIn, DateTime? checkOut, int guests = 1)
    {
        var room = await _uow.Rooms.GetByIdAsync(roomId);
        if (room == null) return NotFound();
        ViewBag.Room = room;
        return View(new CreateReservationDto
        {
            RoomId = roomId,
            CheckIn = checkIn?.Date ?? DateTime.Today.AddDays(1),
            CheckOut = checkOut?.Date ?? DateTime.Today.AddDays(2),
            GuestsCount = Math.Clamp(guests, 1, room.Capacity)
        });
    }

    [HttpGet]
    public IActionResult Track() => View(new ReservationTrackingDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("public-write")]
    public async Task<IActionResult> Track(ReservationTrackingDto model)
    {
        if (!ModelState.IsValid) return View(model);

        var code = model.TrackingCode.Trim().ToUpperInvariant();
        var phone = model.Phone.Trim();
        var reservation = await _uow.Reservations.Query()
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.TrackingCode == code && r.GuestPhone == phone);

        if (reservation == null)
            ModelState.AddModelError(string.Empty, "رزروی با این کد پیگیری و شماره موبایل پیدا نشد.");
        else
            ViewBag.Reservation = reservation;

        return View(model);
    }

    /// <summary>ثبت رزرو</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("public-write")]
    public async Task<IActionResult> Book(CreateReservationDto dto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Room = await _uow.Rooms.GetByIdAsync(dto.RoomId);
            return View(dto);
        }

        var (success, message, trackingCode) = await _reservationService.CreateReservationAsync(dto);
        if (!success)
        {
            ModelState.AddModelError("", message);
            ViewBag.Room = await _uow.Rooms.GetByIdAsync(dto.RoomId);
            return View(dto);
        }

        return RedirectToAction("Confirmation", new { code = trackingCode });
    }

    /// <summary>صفحه تایید رزرو با کد پیگیری</summary>
    public async Task<IActionResult> Confirmation(string code)
    {
        var reservation = await _uow.Reservations.Query()
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.TrackingCode == code);

        if (reservation == null) return NotFound();
        return View(reservation);
    }

    /// <summary>بررسی در دسترس بودن اتاق (AJAX)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("public-write")]
    public async Task<IActionResult> CheckAvailability(int roomId, DateTime checkIn, DateTime checkOut)
    {
        var available = await _reservationService.CheckAvailabilityAsync(roomId, checkIn, checkOut);
        var room = await _uow.Rooms.GetByIdAsync(roomId);

        if (room == null) return Json(new { available = false, message = "اتاق یافت نشد" });

        var nights = (checkOut - checkIn).Days;
        var total = nights * room.PricePerNight;

        return Json(new { available, nights, total = total.ToString("N0"), message = available ? "اتاق در این تاریخ آزاد است" : "اتاق رزرو شده" });
    }

    /// <summary>اعتبارسنجی کد تخفیف (AJAX)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("public-write")]
    public async Task<IActionResult> ValidateDiscount(string code, decimal total)
    {
        var (valid, amount, message) = await _reservationService.ValidateDiscountAsync(code, total);
        return Json(new { valid, amount = amount.ToString("N0"), message });
    }
}
