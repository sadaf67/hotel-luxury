using Hotel.Application.Interfaces;
using Hotel.Application.DTOs;
using Hotel.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace Hotel.Web.Controllers;

/// <summary>کنترلر صفحه اصلی و صفحات عمومی سایت</summary>
public class HomeController : Controller
{
    private readonly IUnitOfWork _uow;

    public HomeController(IUnitOfWork uow) => _uow = uow;

    /// <summary>صفحه اصلی - اسلایدر، اتاق‌های ویژه، امکانات</summary>
    public async Task<IActionResult> Index()
    {
        ViewBag.Sliders = await _uow.Sliders.Query()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        ViewBag.FeaturedRooms = await _uow.Rooms.Query()
            .Where(r => r.Status == Hotel.Domain.Enums.RoomStatus.Available)
            .Include(r => r.Images)
            .Take(3).ToListAsync();

        ViewBag.Settings = await GetSettingsAsync();
        return View();
    }

    /// <summary>صفحه لیست اتاق‌ها با فیلتر</summary>
    public async Task<IActionResult> Rooms(string? type, int? guests, decimal? maxPrice, DateTime? checkIn, DateTime? checkOut)
    {
        var query = _uow.Rooms.Query()
            .Where(r => r.Status != Hotel.Domain.Enums.RoomStatus.Maintenance)
            .Include(r => r.Images)
            .AsQueryable();

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<Hotel.Domain.Enums.RoomType>(type, out var rt))
            query = query.Where(r => r.RoomType == rt);
        if (guests.HasValue)
            query = query.Where(r => r.Capacity >= guests);
        if (maxPrice.HasValue)
            query = query.Where(r => r.PricePerNight <= maxPrice);
        if (checkIn.HasValue && checkOut.HasValue && checkIn.Value.Date >= DateTime.Today && checkOut.Value.Date > checkIn.Value.Date)
        {
            var arrival = checkIn.Value.Date;
            var departure = checkOut.Value.Date;
            query = query.Where(room => !room.Reservations.Any(r =>
                r.Status != Hotel.Domain.Enums.ReservationStatus.Cancelled &&
                r.CheckIn < departure && r.CheckOut > arrival));
        }

        ViewBag.Rooms = await query.OrderBy(r => r.PricePerNight).ToListAsync();
        ViewBag.Settings = await GetSettingsAsync();
        return View();
    }

    /// <summary>صفحه جزئیات اتاق</summary>
    public async Task<IActionResult> RoomDetail(int id)
    {
        var room = await _uow.Rooms.Query()
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();
        ViewBag.Settings = await GetSettingsAsync();
        return View(room);
    }

    /// <summary>صفحه استخر</summary>
    public async Task<IActionResult> Pool()
    {
        ViewBag.Sessions = await _uow.PoolSessions.Query()
            .Where(s => s.IsActive)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
        ViewBag.Settings = await GetSettingsAsync();
        return View();
    }

    /// <summary>صفحه کافی‌شاپ با منوی داینامیک</summary>
    public async Task<IActionResult> Cafe()
    {
        ViewBag.Categories = await _uow.CafeCategories.Query()
            .Where(c => c.IsActive)
            .Include(c => c.MenuItems.Where(m => m.IsAvailable))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
        ViewBag.Settings = await GetSettingsAsync();
        return View();
    }

    /// <summary>صفحه گالری</summary>
    public async Task<IActionResult> Gallery(string? category)
    {
        var query = _uow.Galleries.Query().Where(g => g.IsActive);
        if (!string.IsNullOrEmpty(category))
            query = query.Where(g => g.Category == category);

        ViewBag.Images = await query.OrderBy(g => g.SortOrder).ToListAsync();
        ViewBag.Settings = await GetSettingsAsync();
        return View();
    }

    /// <summary>صفحه تماس با ما</summary>
    public async Task<IActionResult> Contact()
    {
        ViewBag.Settings = await GetSettingsAsync();
        return View(new ContactMessageDto());
    }

    /// <summary>ارسال فرم تماس</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("public-write")]
    public async Task<IActionResult> Contact(ContactMessageDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Settings = await GetSettingsAsync();
            return View(model);
        }

        await _uow.ContactMessages.AddAsync(new ContactMessage
        {
            Name = model.Name.Trim(), Phone = model.Phone.Trim(), Email = model.Email.Trim(),
            Subject = model.Subject.Trim(), Message = model.Message.Trim()
        });
        await _uow.SaveChangesAsync();
        TempData["Success"] = "پیام شما با موفقیت ارسال شد. به زودی با شما تماس می‌گیریم.";
        return RedirectToAction("Contact");
    }

    private async Task<Dictionary<string, string>> GetSettingsAsync()
    {
        var settings = await _uow.SiteSettings.GetAllAsync();
        return settings.ToDictionary(s => s.Key, s => s.Value);
    }
}
