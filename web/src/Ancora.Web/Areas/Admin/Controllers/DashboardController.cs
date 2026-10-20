using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>MODULE 6 — Admin home page with summary cards. URL: /Admin</summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class DashboardController : Controller
{
    private readonly ReportService _reports;
    private readonly AppointmentAdminService _appointments;

    public DashboardController(ReportService reports, AppointmentAdminService appointments)
    {
        _reports = reports;
        _appointments = appointments;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Counts = await _reports.GetDashboardCountsAsync();
        var pending = await _appointments.GetAllAsync(AppointmentStatus.Pending);
        return View(pending.Take(5).ToList());
    }
}
