using Hotel.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public sealed class MessagesController(IUnitOfWork uow) : Controller
{
    public async Task<IActionResult> Index(bool unreadOnly = false)
    {
        var query = uow.ContactMessages.Query();
        if (unreadOnly) query = query.Where(x => !x.IsRead);
        return View(await query.OrderByDescending(x => x.CreatedAt).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var message = await uow.ContactMessages.GetByIdAsync(id);
        if (message == null) return NotFound();
        message.IsRead = true;
        uow.ContactMessages.Update(message);
        await uow.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string reply)
    {
        var message = await uow.ContactMessages.GetByIdAsync(id);
        if (message == null) return NotFound();
        if (string.IsNullOrWhiteSpace(reply) || reply.Length > 2000)
        {
            TempData["Error"] = "متن پاسخ معتبر نیست";
            return RedirectToAction("Index");
        }
        message.AdminReply = reply.Trim();
        message.IsRead = true;
        uow.ContactMessages.Update(message);
        await uow.SaveChangesAsync();
        TempData["Success"] = "پاسخ در پرونده پیام ذخیره شد";
        return RedirectToAction("Index");
    }
}
