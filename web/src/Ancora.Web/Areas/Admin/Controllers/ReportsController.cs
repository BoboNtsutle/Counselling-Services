using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>MODULE 6 — Statistics &amp; reports (FR-11). URL: /Admin/Reports</summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin + "," + Roles.Psychologist)]
public class ReportsController : Controller
{
    private readonly ReportService _service;

    public ReportsController(ReportService service) => _service = service;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var stats = await _service.GetStatisticsAsync(from, to);
        ViewBag.Statistics = stats;
        ViewData["Title"] = "Statistics";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> StatisticsJson(DateTime? from, DateTime? to)
    {
        var stats = await _service.GetStatisticsAsync(from, to);
        return Json(stats);
    }
}
