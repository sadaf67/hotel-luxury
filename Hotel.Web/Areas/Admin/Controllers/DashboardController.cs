using Hotel.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Areas.Admin.Controllers;

/// <summary>داشبورد ادمین - آمار کلی هتل</summary>
[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly ReservationService _reservationService;

    public DashboardController(ReservationService reservationService)
        => _reservationService = reservationService;

    public async Task<IActionResult> Index()
    {
        var stats = await _reservationService.GetDashboardStatsAsync();
        return View(stats);
    }
}
