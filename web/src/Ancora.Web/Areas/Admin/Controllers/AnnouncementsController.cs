using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>MODULE 4 — Announcements, programmes &amp; events (FR-08, FR-12). URL: /Admin/Announcements</summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class AnnouncementsController : Controller
{
    private readonly AnnouncementService _service;

    public AnnouncementsController(AnnouncementService service) => _service = service;

    // DONE (starter): list existing announcements (same ones students see in the app).
    public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string title, string message, IFormFile? MediaFile)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var adminId))
            return Forbid();

        try
        {
            await _service.PublishAsync(adminId, title, message, MediaFile);
            TempData["FlashSuccess"] = "Announcement published.";
        }
        catch (Exception ex)
        {
            TempData["FlashError"] = ex.Message;
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        TempData["FlashSuccess"] = "Announcement deleted.";
        return RedirectToAction("Index");
    }
}
