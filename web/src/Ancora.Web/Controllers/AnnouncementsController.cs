using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Controllers;

/// <summary>Public announcements page visible to any authenticated user (students, psychologists, admins).</summary>
[Authorize]
public class AnnouncementsController : Controller
{
    private readonly AnnouncementService _service;
    public AnnouncementsController(AnnouncementService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var items = await _service.GetAllAsync();
        return View(items);
    }
}
