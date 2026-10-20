using System.Security.Claims;
using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Ancora.Web.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Controllers;

/// <summary>MODULE 1 — "My profile" page for any logged-in staff member.</summary>
public class ProfileController : Controller
{
    private readonly ProfileService _profiles;

    public ProfileController(ProfileService profiles) => _profiles = profiles;

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _profiles.GetProfileAsync(userId);
        var appointments = await _profiles.GetMyAppointmentsAsync(userId);
        // split upcoming and history
        ViewBag.Upcoming = appointments.Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved).ToList();
        ViewBag.History = appointments.Where(a => a.Status == AppointmentStatus.Complete || a.Status == AppointmentStatus.Rejected).ToList();
        return View(profile);
    }

    // GET: /Profile/Edit
    public async Task<IActionResult> Edit()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _profiles.GetProfileAsync(userId);
        if (profile == null) return NotFound();
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string displayName, string phoneNumber, string? bio)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _profiles.UpdateMyProfileAsync(userId, displayName ?? "", phoneNumber ?? "", bio ?? "");
        TempData["FlashSuccess"] = "Profile updated.";
        return RedirectToAction(nameof(Index));
    }

    // TODO (Module 1): add Edit (GET shows a form, POST calls _profiles.UpdateMyProfileAsync,
    // then TempData[Flash.Success] = "Profile updated." and redirect to Index).

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        try
        {
            await _profiles.CancelAppointmentAsync(id, userId);
            TempData["FlashSuccess"] = "Appointment cancelled.";
        }
        catch (SupabaseException ex)
        {
            TempData["FlashError"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
