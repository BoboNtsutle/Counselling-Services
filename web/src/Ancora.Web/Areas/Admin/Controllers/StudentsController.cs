using Ancora.Web.Models;
using Ancora.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Admin.Controllers;

/// <summary>
/// Student records (read only): search the list, then open one student to see their details
/// and appointment history. URL: /Admin/Students
/// The Admin area and the Psychologist area share the same two pages (Views/Shared/StudentRecords*.cshtml).
/// </summary>
[Area("Admin")]
[Authorize(Roles = Roles.Admin)]
public class StudentsController : Controller
{
    private readonly UserAdminService _service;

    public StudentsController(UserAdminService service) => _service = service;

    // Search box: by name or student number (or either). Empty search = all students.
    public async Task<IActionResult> Index(string? q, string? by)
    {
        var model = await _service.SearchStudentsAsync(q, by);
        return View("~/Views/Shared/StudentRecords.cshtml", model);
    }

    // One student's record.
    public async Task<IActionResult> Details(Guid id)
    {
        var record = await _service.GetStudentRecordAsync(id);
        if (record == null) return NotFound();
        return View("~/Views/Shared/StudentRecord.cshtml", record);
    }
}
