using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>MODULE 3 — Users, role requests &amp; student records (FR-06, FR-07, FR-10). URL: /Admin/Users</summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class UsersController : Controller
{
    private readonly UserAdminService _service;

    public UsersController(UserAdminService service) => _service = service;

    // DONE (starter): pending role requests + list of psychologists.
    public async Task<IActionResult> Index()
    {
        ViewBag.Psychologists = await _service.GetByRoleAsync(Roles.Psychologist);
        var pending = await _service.GetPendingRoleRequestsAsync();
        return View(pending);
    }

    // TODO (Module 3):
    // [HttpPost, ValidateAntiForgeryToken] Approve(Guid id, string role)
    // [HttpPost, ValidateAntiForgeryToken] Reject(Guid id)
    // GET Students(string? q)      → search page using _service.SearchStudentsAsync(q)   (FR-07)
    // GET Student(Guid id)         → one student's record + appointment history         (FR-06)
}
