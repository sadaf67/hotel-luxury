using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class CafeController : Controller
{
    private readonly IUnitOfWork _uow;
    public CafeController(IUnitOfWork uow) => _uow = uow;
    public async Task<IActionResult> Index() => View(await _uow.CafeCategories.Query().Include(x=>x.MenuItems).OrderBy(x=>x.SortOrder).ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string name) { if(string.IsNullOrWhiteSpace(name)){TempData["Error"]="نام دسته الزامی است";return RedirectToAction("Index");} await _uow.CafeCategories.AddAsync(new CafeCategory{Name=name.Trim()}); await _uow.SaveChangesAsync(); return RedirectToAction("Index"); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(CafeMenuItem model) { if(string.IsNullOrWhiteSpace(model.Name)||model.Price<0){TempData["Error"]="نام و قیمت معتبر الزامی است";return RedirectToAction("Index");} await _uow.CafeMenuItems.AddAsync(model); await _uow.SaveChangesAsync(); TempData["Success"]="آیتم منو اضافه شد"; return RedirectToAction("Index"); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id){var item=await _uow.CafeMenuItems.GetByIdAsync(id);if(item==null)return NotFound();_uow.CafeMenuItems.Remove(item);await _uow.SaveChangesAsync();return Json(new{success=true});}
}
