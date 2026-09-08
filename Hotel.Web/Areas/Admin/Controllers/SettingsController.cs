using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Areas.Admin.Controllers;

/// <summary>مدیریت تنظیمات سایت - اسلایدر، محتوا، اطلاعات تماس</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadsRoot;

    public SettingsController(IUnitOfWork uow, IWebHostEnvironment env, IConfiguration configuration)
    {
        _uow = uow;
        _env = env;
        _uploadsRoot = configuration["Storage:UploadsPath"] ?? Path.Combine(_env.WebRootPath, "uploads");
    }

    /// <summary>تنظیمات عمومی سایت</summary>
    public async Task<IActionResult> Index()
    {
        var settings = await _uow.SiteSettings.GetAllAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSettings(Dictionary<string, string> settings)
    {
        foreach (var kv in settings)
        {
            var setting = await _uow.SiteSettings.FirstOrDefaultAsync(s => s.Key == kv.Key);
            if (setting != null)
            {
                setting.Value = kv.Value;
                _uow.SiteSettings.Update(setting);
            }
        }
        await _uow.SaveChangesAsync();
        TempData["Success"] = "تنظیمات ذخیره شد";
        return RedirectToAction("Index");
    }

    /// <summary>مدیریت اسلایدرها</summary>
    public async Task<IActionResult> Sliders()
    {
        var sliders = await _uow.Sliders.GetAllAsync();
        return View(sliders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSlider(Slider model, IFormFile image)
    {
        if (image != null)
        {
            if (!IsValidImage(image)) { TempData["Error"] = "تصویر باید JPG، PNG یا WebP و حداکثر ۵ مگابایت باشد"; return RedirectToAction("Sliders"); }
            var uploadsPath = Path.Combine(_uploadsRoot, "sliders");
            Directory.CreateDirectory(uploadsPath);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            using var stream = new FileStream(Path.Combine(uploadsPath, fileName), FileMode.Create);
            await image.CopyToAsync(stream);
            model.ImagePath = $"/uploads/sliders/{fileName}";
        }
        await _uow.Sliders.AddAsync(model);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "اسلاید اضافه شد";
        return RedirectToAction("Sliders");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSlider(int id)
    {
        var slider = await _uow.Sliders.GetByIdAsync(id);
        if (slider != null) { _uow.Sliders.Remove(slider); await _uow.SaveChangesAsync(); }
        return Json(new { success = true });
    }

    /// <summary>مدیریت گالری</summary>
    public async Task<IActionResult> Gallery()
    {
        var images = await _uow.Galleries.GetAllAsync();
        return View(images);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGalleryImage(IFormFile image, string title, string? category)
    {
        if (image == null) return Json(new { success = false });
        if (!IsValidImage(image)) { TempData["Error"] = "فایل تصویر معتبر نیست"; return RedirectToAction("Gallery"); }

        var uploadsPath = Path.Combine(_uploadsRoot, "gallery");
        Directory.CreateDirectory(uploadsPath);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        using var stream = new FileStream(Path.Combine(uploadsPath, fileName), FileMode.Create);
        await image.CopyToAsync(stream);

        await _uow.Galleries.AddAsync(new Gallery
        {
            Title = title,
            ImagePath = $"/uploads/gallery/{fileName}",
            Category = category
        });
        await _uow.SaveChangesAsync();
        TempData["Success"] = "تصویر به گالری اضافه شد";
        return RedirectToAction("Gallery");
    }

    private static bool IsValidImage(IFormFile file)
    {
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        return file.Length > 0 && file.Length <= 5 * 1024 * 1024 && allowed.Contains(Path.GetExtension(file.FileName));
    }
}
