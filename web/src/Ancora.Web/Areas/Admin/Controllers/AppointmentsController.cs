using Ancora.Web.Models;
using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>MODULE 2 — Admin appointment management (FR-03, FR-04). URL: /Admin/Appointments</summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class AppointmentsController : Controller
{
    private readonly AppointmentAdminService _service;

    public AppointmentsController(AppointmentAdminService service) => _service = service;

    // DONE (starter): list + filter by status.
    public async Task<IActionResult> Index(string? status)
    {
        ViewBag.Status = status;
        var appointments = await _service.GetAllAsync(status);
        return View(appointments);
    }

    // DONE (starter): details page.
    public async Task<IActionResult> Details(Guid id)
    {
        var appointment = await _service.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        ViewBag.Psychologists = await _service.GetPsychologistsAsync();
        return View(appointment);
    }

    // TODO (Module 2):
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, Guid psychologistId)
    {
        try
        {
            await _service.AssignAndApproveAsync(id, psychologistId);
            TempData[Flash.Success] = "Appointment approved.";
        }
        catch (InvalidOperationException ex)
        {
            TempData[Flash.Error] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id)
    {
        await _service.RejectAsync(id);
        TempData[Flash.Success] = "Appointment rejected.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(Guid id, string newDate, string newTime)
    {
        try
        {
            if (DateTime.TryParse(newDate, out var day) && !WorkingHours.IsWorkingDay(day))
                throw new InvalidOperationException("The centre is open Monday to Friday. Please pick a weekday.");
            await _service.RescheduleAsync(id, newDate, newTime);
            TempData[Flash.Success] = "Appointment rescheduled.";
        }
        catch (InvalidOperationException ex)
        {
            TempData[Flash.Error] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id });
    }
}
