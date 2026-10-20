using Ancora.Web.Models;
using Ancora.Web.Models.ViewModels;
using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Student.Controllers;

/// <summary>"Book a session" page for students. URL: /Student/Bookings</summary>
[Area("Student")]
[Authorize(Roles = Roles.Student)]
public class BookingsController : Controller
{
    private readonly ProfileService _profiles;

    public BookingsController(ProfileService profiles) => _profiles = profiles;

    // GET: show the empty form.
    public async Task<IActionResult> Index()
    {
        var profile = await _profiles.GetProfileAsync(User.GetUserId());
        var form = new BookSessionViewModel { AppointmentDate = DateTime.Today.AddDays(1) };
        FillStudentDetails(form, profile);
        return View(form);
    }

    // POST: check the form, save the request, go back to the dashboard.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookSessionViewModel form)
    {
        var profile = await _profiles.GetProfileAsync(User.GetUserId());
        if (profile == null)
        {
            TempData[Flash.Error] = "Your profile could not be loaded. Please sign in again.";
            return RedirectToAction("Index", "Dashboard");
        }
        FillStudentDetails(form, profile);

        // Extra checks the [Required] attributes cannot do.
        if (form.AppointmentDate.HasValue && form.AppointmentDate.Value.Date < DateTime.Today)
            ModelState.AddModelError(nameof(form.AppointmentDate), "Please choose today or a later date.");
        else if (form.AppointmentDate.HasValue && !WorkingHours.IsWorkingDay(form.AppointmentDate.Value))
            ModelState.AddModelError(nameof(form.AppointmentDate), "The centre is open Monday to Friday. Please pick a weekday.");

        if (form.AppointmentTime != null && !WorkingHours.Slots.Contains(form.AppointmentTime))
            ModelState.AddModelError(nameof(form.AppointmentTime), "Please choose one of the listed times.");

        if (form.ProblemCategory != null && !BookSessionViewModel.Categories.Contains(form.ProblemCategory))
            ModelState.AddModelError(nameof(form.ProblemCategory), "Please choose one of the listed topics.");

        // Never trust the posted urgency value; fall back to Routine.
        if (form.Urgency == null || !BookSessionViewModel.UrgencyLevels.Contains(form.Urgency))
            form.Urgency = Urgency.Routine;

        if (!ModelState.IsValid) return View("Index", form);

        await _profiles.BookAppointmentAsync(profile, form);
        TempData[Flash.Success] = "Your request was sent. You will see it on your dashboard once the centre approves it.";
        return RedirectToAction("Index", "Dashboard");
    }

    // Name, student number and course always come from the profile, not from the form.
    private static void FillStudentDetails(BookSessionViewModel form, Profile? profile)
    {
        form.StudentName = profile?.DisplayName ?? "";
        form.StudentNumber = profile?.StudentNumber ?? "";
        form.Course = profile?.Course ?? "";
    }
}
