using Ancora.Web.Services;
using Ancora.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Psychologist.Controllers;

[Area("Psychologist")]
[Authorize(Roles = Roles.Psychologist)]
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
