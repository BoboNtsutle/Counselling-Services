using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Psychologist.Controllers;

/// <summary>MODULE 5 — One appointment: status update + session notes (FR-05, FR-09). URL: /Psychologist/Appointments/Details/{id}</summary>
[Area("Psychologist")]
[Authorize(Roles = Roles.Psychologist)]
public class AppointmentsController : Controller
{
    private readonly PsychologistService _service;
    private readonly AppointmentAdminService _admin;

    public AppointmentsController(PsychologistService service, AppointmentAdminService admin)
    {
        _service = service;
        _admin = admin;
    }

    // DONE (starter): details of one of MY appointments.
    public async Task<IActionResult> Details(Guid id)
    {
        var appointment = await _service.GetMyAppointmentAsync(User.GetUserId(), id);
        if (appointment == null) return NotFound();
        ViewBag.Notes = await _service.GetNotesAsync(id);
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        try
        {
            await _service.UpdateStatusAsync(id, User.GetUserId(), status);
            TempData["FlashSuccess"] = "Status updated.";
        }
        catch (Exception ex)
        {
            TempData["FlashError"] = ex.Message;
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNotes(Guid id, string notes)
    {
        try
        {
            await _service.SaveNotesAsync(id, notes ?? "");
            TempData["FlashSuccess"] = "Notes saved.";
        }
        catch (Exception ex)
        {
            TempData["FlashError"] = ex.Message;
        }
        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(Guid id, DateTime? newDate, TimeSpan? newTime)
    {
        try
        {
            // Validate inputs
            if (!newDate.HasValue || !newTime.HasValue)
                throw new InvalidOperationException("Please provide both date and time.");

            // Ensure the appointment belongs to this psychologist
            var ap = await _service.GetMyAppointmentAsync(User.GetUserId(), id);
            if (ap == null) return NotFound();
            if (ap.Status != Ancora.Web.Models.AppointmentStatus.Approved)
                throw new InvalidOperationException("Only approved appointments can be rescheduled.");

            var date = newDate.Value.ToString("yyyy-MM-dd");
            var time = newTime.Value.ToString(@"hh\:mm");

            // Only future days, and only the centre's working hours (08:00 - 16:00).
            if (newDate.Value.Date < DateTime.Today)
                throw new InvalidOperationException("Please choose today or a later date.");
            if (!WorkingHours.IsWorkingDay(newDate.Value))
                throw new InvalidOperationException("The centre is open Monday to Friday. Please pick a weekday.");
            if (!WorkingHours.Slots.Contains(time))
                throw new InvalidOperationException("Please choose one of the listed times.");

            // Use admin service to reschedule which includes clash checking
            await _admin.RescheduleAsync(id, date, time);
            TempData["FlashSuccess"] = "Appointment rescheduled.";
        }
        catch (Exception ex)
        {
            TempData["FlashError"] = ex.Message;
        }
        return RedirectToAction("Details", new { id });
    }
}
