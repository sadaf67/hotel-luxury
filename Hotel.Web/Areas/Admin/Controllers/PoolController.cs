using Hotel.Application.Interfaces;
using Hotel.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public class PoolController : Controller
{
    private readonly IUnitOfWork _uow;
    public PoolController(IUnitOfWork uow) => _uow = uow;
    public async Task<IActionResult> Index() => View(await _uow.PoolSessions.Query().OrderBy(x => x.StartTime).ToListAsync());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(PoolSession model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "اطلاعات سانس کامل نیست"; return RedirectToAction("Index"); }
        if (model.Id == 0) await _uow.PoolSessions.AddAsync(model);
        else { var item = await _uow.PoolSessions.GetByIdAsync(model.Id); if (item == null) return NotFound(); item.Title=model.Title; item.StartTime=model.StartTime; item.EndTime=model.EndTime; item.MaxCapacity=model.MaxCapacity; item.AdultPrice=model.AdultPrice; item.ChildPrice=model.ChildPrice; item.IsActive=model.IsActive; item.Description=model.Description; _uow.PoolSessions.Update(item); }
        await _uow.SaveChangesAsync(); TempData["Success"] = "سانس استخر ذخیره شد"; return RedirectToAction("Index");
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id) { var item=await _uow.PoolSessions.GetByIdAsync(id); if(item==null)return NotFound(); _uow.PoolSessions.Remove(item); await _uow.SaveChangesAsync(); return Json(new{success=true}); }
}
