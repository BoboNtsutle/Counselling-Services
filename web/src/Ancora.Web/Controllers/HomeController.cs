using System.Diagnostics;
using Ancora.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Controllers;

public class HomeController : Controller
{
    /// <summary>After login, send each role to its own dashboard.</summary>
    public IActionResult Index()
    {
        if (User.IsInRole(Roles.Admin)) return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        if (User.IsInRole(Roles.Psychologist)) return RedirectToAction("Index", "Dashboard", new { area = "Psychologist" });
        if (User.IsInRole(Roles.Student)) return RedirectToAction("Index", "Dashboard", new { area = "Student" });
        // Any other authenticated user lands on their profile page.
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Profile");
        return RedirectToAction("AccessDenied", "Account");
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
