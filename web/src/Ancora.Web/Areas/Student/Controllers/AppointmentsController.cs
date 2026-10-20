using Ancora.Web.Models;
using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Student.Controllers;

/// <summary>The student's own appointments (current ones, with a cancel button). URL: /Student/Appointments</summary>
[Area("Student")]
[Authorize(Roles = Roles.Student)]
public class AppointmentsController : Controller
{
    private readonly ProfileService _profiles;

    public AppointmentsController(ProfileService profiles) => _profiles = profiles;

    public async Task<IActionResult> Index()
    {
        var all = await _profiles.GetMyAppointmentsAsync(User.GetUserId());
        var current = all
            .Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved)
            .ToList();
        return View(current);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        // The service only changes the row if it belongs to this student.
        await _profiles.CancelAppointmentAsync(id, User.GetUserId());
        TempData[Flash.Success] = "Appointment cancelled.";
        return RedirectToAction(nameof(Index));
    }
}
