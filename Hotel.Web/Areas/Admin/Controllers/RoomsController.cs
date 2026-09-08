using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Hotel.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.Admin.Controllers;

/// <summary>مدیریت اتاق‌های هتل</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RoomsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsRoot;

    public RoomsController(IUnitOfWork uow, IWebHostEnvironment env, IConfiguration configuration)
    {
        _uow = uow;
        _env = env;
        _uploadsRoot = configuration["Storage:UploadsPath"] ?? Path.Combine(_env.WebRootPath, "uploads");
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _uow.Rooms.Query().Include(r => r.Images).ToListAsync();
        return View(rooms);
    }

    public IActionResult Create() => View(new Room());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Room model, IFormFile? mainImage)
    {
        if (!ModelState.IsValid) return View(model);

        // آپلود تصویر اصلی
        if (mainImage != null)
            model.MainImagePath = await SaveImageAsync(mainImage);

        await _uow.Rooms.AddAsync(model);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "اتاق با موفقیت اضافه شد";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var room = await _uow.Rooms.Query().Include(r => r.Images).FirstOrDefaultAsync(r => r.Id == id);
        if (room == null) return NotFound();
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Room model, IFormFile? mainImage, List<IFormFile>? galleryImages)
    {
        if (!ModelState.IsValid) return View(model);

        var room = await _uow.Rooms.GetByIdAsync(model.Id);
        if (room == null) return NotFound();

        // به‌روزرسانی فیلدها
        room.RoomNumber = model.RoomNumber;
        room.Name = model.Name;
        room.Description = model.Description;
        room.RoomType = model.RoomType;
        room.Status = model.Status;
        room.Capacity = model.Capacity;
        room.PricePerNight = model.PricePerNight;
        room.Floor = model.Floor;
        room.HasBalcony = model.HasBalcony;
        room.HasJacuzzi = model.HasJacuzzi;
        room.HasMinibar = model.HasMinibar;

        if (mainImage != null)
            room.MainImagePath = await SaveImageAsync(mainImage);

        // آپلود تصاویر گالری
        if (galleryImages != null && galleryImages.Any())
        {
            foreach (var img in galleryImages)
            {
                var path = await SaveImageAsync(img);
                await _uow.RoomImages.AddAsync(new RoomImage { RoomId = room.Id, ImagePath = path });
            }
        }

        _uow.Rooms.Update(room);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "اتاق با موفقیت ویرایش شد";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _uow.Rooms.GetByIdAsync(id);
        if (room == null) return NotFound();
        _uow.Rooms.Remove(room);
        await _uow.SaveChangesAsync();
        return Json(new { success = true });
    }

    /// <summary>ذخیره تصویر در wwwroot/uploads</summary>
    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName);
        if (!allowedExtensions.Contains(extension) || file.Length == 0 || file.Length > 5 * 1024 * 1024)
            throw new InvalidOperationException("تصویر باید JPG، PNG یا WebP و حداکثر ۵ مگابایت باشد.");
        var uploadsPath = Path.Combine(_uploadsRoot, "rooms");
        Directory.CreateDirectory(uploadsPath);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/rooms/{fileName}";
    }
}
