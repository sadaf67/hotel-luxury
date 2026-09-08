using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Areas.Admin.Controllers;

/// <summary>پنل مدیریت کدهای تخفیف</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DiscountsController : Controller
{
    private readonly IUnitOfWork _uow;

    public DiscountsController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index()
    {
        var discounts = await _uow.Discounts.GetAllAsync();
        return View(discounts);
    }

    public IActionResult Create() => View(new Discount());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Discount model)
    {
        if (!ModelState.IsValid) return View(model);

        // کد تخفیف به بزرگ
        model.Code = model.Code.ToUpper().Trim();
        await _uow.Discounts.AddAsync(model);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "کد تخفیف با موفقیت ایجاد شد";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var discount = await _uow.Discounts.GetByIdAsync(id);
        if (discount == null) return NotFound();
        return View(discount);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Discount model)
    {
        if (!ModelState.IsValid) return View(model);

        var discount = await _uow.Discounts.GetByIdAsync(model.Id);
        if (discount == null) return NotFound();

        discount.Code = model.Code.ToUpper().Trim();
        discount.Title = model.Title;
        discount.Type = model.Type;
        discount.Value = model.Value;
        discount.MinimumAmount = model.MinimumAmount;
        discount.MaximumDiscount = model.MaximumDiscount;
        discount.StartDate = model.StartDate;
        discount.EndDate = model.EndDate;
        discount.MaxUsageCount = model.MaxUsageCount;
        discount.IsActive = model.IsActive;
        discount.Description = model.Description;

        _uow.Discounts.Update(discount);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "کد تخفیف به‌روزرسانی شد";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var d = await _uow.Discounts.GetByIdAsync(id);
        if (d == null) return Json(new { success = false });
        d.IsActive = !d.IsActive;
        _uow.Discounts.Update(d);
        await _uow.SaveChangesAsync();
        return Json(new { success = true, isActive = d.IsActive });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _uow.Discounts.GetByIdAsync(id);
        if (d == null) return Json(new { success = false });
        _uow.Discounts.Remove(d);
        await _uow.SaveChangesAsync();
        return Json(new { success = true });
    }
}
