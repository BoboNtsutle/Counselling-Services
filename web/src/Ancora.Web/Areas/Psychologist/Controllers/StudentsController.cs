using Ancora.Web.Models;
using Ancora.Web.Services;
using Ancora.Web.Services.Supabase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ancora.Web.Areas.Psychologist.Controllers;

/// <summary>
/// Student records for psychologists. URL: /Psychologist/Students
/// A psychologist can see EVERY student (not only their own clients), search by name or
/// student number, open a student's full record, and save entries on it that stay on the file.
/// The Psychologist and Admin areas share the same two pages (Views/Shared/StudentRecords*.cshtml);
/// only psychologists get the "add entry" form.
/// </summary>
[Area("Psychologist")]
[Authorize(Roles = Roles.Psychologist)]
public class StudentsController : Controller
{
    private const int MaxTitle = 150;
    private const int MaxNotes = 8000;

    private readonly UserAdminService _service;

    public StudentsController(UserAdminService service) => _service = service;

    // List + search. by = name | number | any (default). Empty search = all students.
    public async Task<IActionResult> Index(string? q, string? by)
    {
        var model = await _service.SearchStudentsAsync(q, by);
        return View("~/Views/Shared/StudentRecords.cshtml", model);
    }

    // One student's full record.
    public async Task<IActionResult> Details(Guid id)
    {
        var record = await _service.GetStudentRecordAsync(id);
        if (record == null) return NotFound();
        return View("~/Views/Shared/StudentRecord.cshtml", record);
    }

    // Save a new entry on the student's record. Entries are kept permanently.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEntry(Guid id, string? category, string? title, string? notes, Guid? appointmentId)
    {
        var error = Validate(ref category, ref title, ref notes);
        if (error != null)
        {
            TempData[Flash.Error] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        var record = await _service.GetStudentRecordAsync(id);
        if (record == null) return NotFound();

        // Only link the entry to an appointment that really belongs to this student.
        if (appointmentId.HasValue && record.Appointments.All(a => a.Id != appointmentId.Value))
            appointmentId = null;

        await _service.AddRecordEntryAsync(record.Student, User.GetUserId(), AuthorName(),
            appointmentId, category!, title!, notes!);

        TempData[Flash.Success] = "Saved to the student's record.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // Edit an entry you wrote. Nothing is ever deleted from the web app.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEntry(Guid id, Guid entryId, string? category, string? title, string? notes)
    {
        var error = Validate(ref category, ref title, ref notes);
        if (error != null)
        {
            TempData[Flash.Error] = error;
            return RedirectToAction(nameof(Details), new { id });
        }

        await _service.UpdateRecordEntryAsync(entryId, User.GetUserId(), category!, title!, notes!);
        TempData[Flash.Success] = "Record entry updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private string AuthorName() =>
        string.IsNullOrWhiteSpace(User.Identity?.Name) ? "Psychologist" : User.Identity!.Name!;

    private static string? Validate(ref string? category, ref string? title, ref string? notes)
    {
        category = StudentRecordCategories.All.Contains(category ?? "") ? category : StudentRecordCategories.General;
        title = (title ?? "").Trim();
        notes = (notes ?? "").Trim();

        if (title.Length == 0) return "Please give the entry a title.";
        if (title.Length > MaxTitle) return $"The title can be at most {MaxTitle} characters.";
        if (notes.Length == 0) return "Please write the record notes before saving.";
        if (notes.Length > MaxNotes) return $"The notes can be at most {MaxNotes} characters.";
        return null;
    }
}
