using Ancora.Web.Models;
using Ancora.Web.Models.Admin;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>Admin: monthly calendar showing when a psychologist is free or booked. URL: /Admin/Availability</summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class AvailabilityController : Controller
{
    private readonly AppointmentAdminService _appointments;

    public AvailabilityController(AppointmentAdminService appointments) => _appointments = appointments;

    // Current month and first psychologist unless ?year=&month=&psychologistId= are given.
    public async Task<IActionResult> Index(int? year, int? month, Guid? psychologistId)
    {
        var today = DateTime.Today;
        var y = year ?? today.Year;
        var m = month ?? today.Month;
        if (m < 1 || m > 12 || y < 2000 || y > 2100) { y = today.Year; m = today.Month; }

        var psychologists = await _appointments.GetPsychologistsAsync();
        var approved = await _appointments.GetAllAsync(AppointmentStatus.Approved);   // only Approved blocks a slot

        return View(AvailabilityViewModel.ForMonth(y, m, psychologists, psychologistId, approved));
    }
}
