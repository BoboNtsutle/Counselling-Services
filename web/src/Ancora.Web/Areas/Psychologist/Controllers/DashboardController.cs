using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Psychologist.Controllers;

/// <summary>MODULE 5 — Psychologist home: my assigned appointments (FR-05). URL: /Psychologist</summary>
[Area("Psychologist")]
[Authorize(Roles = Roles.Psychologist)]
public class DashboardController : Controller
{
    private readonly PsychologistService _service;

    public DashboardController(PsychologistService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var mine = await _service.GetMyAppointmentsAsync(User.GetUserId());
        return View(mine);
    }
}
