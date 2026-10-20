using Ancora.Web.Models;
using Ancora.Web.Models.ViewModels;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Student.Controllers;

/// <summary>
/// Student home page. URL: /Student
/// Shows the student's current appointments and the latest news the admin published.
/// (Supabase errors are handled for us by SupabaseErrorFilter, so no try/catch is needed.)
/// </summary>
[Area("Student")]
[Authorize(Roles = Roles.Student)]
public class DashboardController : Controller
{
    private readonly ProfileService _profiles;
    private readonly AnnouncementService _announcements;

    public DashboardController(ProfileService profiles, AnnouncementService announcements)
    {
        _profiles = profiles;
        _announcements = announcements;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();

        // This student's appointments, oldest date first.
        var mine = await _profiles.GetMyAppointmentsAsync(userId);

        var model = new StudentDashboardViewModel
        {
            Profile = await _profiles.GetProfileAsync(userId),

            // "Current" = still waiting for approval, or approved. Finished/rejected ones are left out.
            Upcoming = mine
                .Where(a => a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved)
                .ToList(),

            // Newest announcements first (the service already sorts them); show the latest three.
            News = (await _announcements.GetAllAsync()).Take(3).ToList()
        };

        return View(model);
    }
}
