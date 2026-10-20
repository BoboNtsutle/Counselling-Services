using Ancora.Web.Models;
using Ancora.Web.Models.ViewModels;
using Ancora.Web.Services.Supabase;

namespace Ancora.Web.Services;

/// <summary>
/// MODULE 3 — Admin: Users, role requests &amp; student records (FR-06, FR-07, FR-10)
/// Owner: see TEAM-PLAN.md
/// </summary>
public class UserAdminService
{
    private readonly SupabaseClient _db;

    public UserAdminService(SupabaseClient db) => _db = db;

    /// <summary>People who registered in the Flutter app and asked to be a psychologist or admin.</summary>
    public Task<List<Profile>> GetPendingRoleRequestsAsync() =>
        _db.GetAsync<Profile>("profiles", "role_request_status=eq.pending&order=created_at");

    public Task<List<Profile>> GetByRoleAsync(string role) =>
        _db.GetAsync<Profile>("profiles", $"role=eq.{role}&order=display_name");

    // TODO (Module 3): approve a role request (same as reviewRoleRequest in the Flutter app).
    //   await _db.UpdateAsync("profiles", $"id=eq.{userId}",
    //       new { role = approvedRole, role_request_status = "approved" });
    public Task ApproveRoleRequestAsync(Guid userId, string approvedRole) =>
        throw new NotImplementedException("Module 3: ApproveRoleRequestAsync");

    // TODO (Module 3): reject → role = "student", role_request_status = "rejected"
    public Task RejectRoleRequestAsync(Guid userId) =>
        throw new NotImplementedException("Module 3: RejectRoleRequestAsync");

    // ------------------------------------------------------------------
    //  Student records (FR-06, FR-07)
    //  The list is every student the centre knows about: people with a student profile
    //  PLUS anyone who has booked a session (so nobody who shows up in the Student side
    //  of the system is missing from the records page).
    // ------------------------------------------------------------------

    /// <summary>
    /// FR-07: list / search students by name or student number.
    /// searchBy = "name", "number" or "any" (name, number or email). An empty search returns everyone.
    /// </summary>
    public async Task<StudentListViewModel> SearchStudentsAsync(string? term, string? searchBy = null)
    {
        var by = StudentSearchBy.Normalise(searchBy);

        var profiles = await _db.GetAsync<Profile>("profiles",
            $"role=eq.{Roles.Student}&order=display_name&limit=5000");

        // Only the columns we need, so this stays light even with many appointments.
        var appointments = await _db.GetAsync<Appointment>("appointments",
            "select=id,student_id,student_name,student_number,course,appointment_date,appointment_time,created_at&order=created_at.desc&limit=10000");

        var (entries, tableMissing) = await TryGetEntryCountsAsync();

        var items = new Dictionary<Guid, StudentListItem>();
        foreach (var p in profiles)
        {
            items[p.Id] = new StudentListItem
            {
                Id = p.Id, DisplayName = p.DisplayName, StudentNumber = p.StudentNumber, Course = p.Course,
                Email = p.Email, PhoneNumber = p.PhoneNumber, HasProfile = true
            };
        }

        // appointments are newest first, so the first one we see per student is their latest session.
        foreach (var a in appointments)
        {
            if (!items.TryGetValue(a.StudentId, out var item))
            {
                // Booked a session but has no student profile row we can read: still list them.
                item = new StudentListItem
                {
                    Id = a.StudentId, DisplayName = a.StudentName, StudentNumber = a.StudentNumber,
                    Course = a.Course, HasProfile = false
                };
                items[a.StudentId] = item;
            }
            item.AppointmentCount++;
            if (item.LastSession == "") item.LastSession = $"{a.AppointmentDate} {a.AppointmentTime}".Trim();
            // Fill gaps in the profile from the booking details.
            if (string.IsNullOrWhiteSpace(item.DisplayName)) item.DisplayName = a.StudentName;
            if (string.IsNullOrWhiteSpace(item.StudentNumber)) item.StudentNumber = a.StudentNumber;
            if (string.IsNullOrWhiteSpace(item.Course)) item.Course = a.Course;
        }

        foreach (var e in entries)
            if (items.TryGetValue(e.StudentId, out var item)) item.RecordCount++;

        var all = items.Values.OrderBy(i => i.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
        var matches = string.IsNullOrWhiteSpace(term) ? all : all.Where(i => Matches(i, term, by)).ToList();

        return new StudentListViewModel
        {
            Students = matches, TotalStudents = all.Count, Query = term, SearchBy = by, RecordsTableMissing = tableMissing
        };
    }

    private static bool Matches(StudentListItem s, string term, string by)
    {
        var t = term.Trim();
        // Student numbers are compared without spaces or dashes ("21 2345-678" finds "212345678").
        static string Digits(string x) => new string(x.Where(char.IsLetterOrDigit).ToArray());

        bool NameMatch() =>
            t.Split(' ', StringSplitOptions.RemoveEmptyEntries).All(tok =>
                (s.DisplayName ?? "").Contains(tok.Trim(','), StringComparison.CurrentCultureIgnoreCase));
        bool NumberMatch()
        {
            var want = Digits(t);
            return want.Length > 0 && Digits(s.StudentNumber ?? "").Contains(want, StringComparison.CurrentCultureIgnoreCase);
        }

        return by switch
        {
            StudentSearchBy.Name => NameMatch(),
            StudentSearchBy.Number => NumberMatch(),
            _ => NameMatch() || NumberMatch() || (s.Email ?? "").Contains(t, StringComparison.CurrentCultureIgnoreCase)
        };
    }

    /// <summary>
    /// FR-06: one student's full record = profile + every appointment + every saved record entry
    /// + the session notes the signed-in user may read. Returns null when we know nothing about this id.
    /// </summary>
    public async Task<StudentRecordViewModel?> GetStudentRecordAsync(Guid studentId)
    {
        var profile = await _db.GetSingleAsync<Profile>("profiles", $"id=eq.{studentId}&role=eq.{Roles.Student}");

        var appointments = await _db.GetAsync<Appointment>("appointments",
            $"student_id=eq.{studentId}&order=created_at.desc");

        var hasProfile = profile != null;
        if (profile == null)
        {
            // Not a student profile we can read, but they may still have booked sessions.
            var latest = appointments.FirstOrDefault();
            if (latest == null) return null;
            profile = new Profile
            {
                Id = studentId, DisplayName = latest.StudentName, StudentNumber = latest.StudentNumber,
                Course = latest.Course, CreatedAt = appointments.Last().CreatedAt
            };
        }

        var vm = new StudentRecordViewModel { Student = profile, Appointments = appointments, HasProfile = hasProfile };

        try
        {
            vm.Entries = await _db.GetAsync<StudentRecordEntry>("student_records",
                $"student_id=eq.{studentId}&order=created_at.desc");
        }
        catch (SupabaseException ex) when (IsMissingTable(ex))
        {
            vm.RecordsTableMissing = true;
        }

        if (appointments.Count > 0)
        {
            var ids = string.Join(",", appointments.Select(a => a.Id));
            var notes = await _db.GetAsync<ClinicalNote>("clinical_notes", $"appointment_id=in.({ids})");
            vm.SessionNotes = notes
                .Where(n => !string.IsNullOrWhiteSpace(n.Notes))
                .GroupBy(n => n.AppointmentId)
                .ToDictionary(g => g.Key, g => g.First().Notes);
        }

        return vm;
    }

    /// <summary>Saves a new entry on a student's permanent record.</summary>
    public Task<StudentRecordEntry?> AddRecordEntryAsync(Profile student, Guid authorId, string authorName,
        Guid? appointmentId, string category, string title, string notes)
    {
        var now = DateTimeOffset.UtcNow;
        return _db.InsertAsync<StudentRecordEntry>("student_records", new
        {
            student_id = student.Id,
            student_name = student.DisplayName,
            student_number = student.StudentNumber,
            appointment_id = appointmentId,
            author_id = authorId,
            author_name = authorName,
            category,
            title,
            notes,
            created_at = now,
            updated_at = now
        });
    }

    /// <summary>Changes an entry. Only the psychologist who wrote it can edit it (the filter and the database both enforce this).</summary>
    public Task UpdateRecordEntryAsync(Guid entryId, Guid authorId, string category, string title, string notes) =>
        _db.UpdateAsync("student_records", $"id=eq.{entryId}&author_id=eq.{authorId}",
            new { category, title, notes, updated_at = DateTimeOffset.UtcNow });

    // ---- helpers ----

    private async Task<(List<StudentRecordEntry> Entries, bool TableMissing)> TryGetEntryCountsAsync()
    {
        try
        {
            var rows = await _db.GetAsync<StudentRecordEntry>("student_records", "select=id,student_id&limit=20000");
            return (rows, false);
        }
        catch (SupabaseException ex) when (IsMissingTable(ex))
        {
            return (new List<StudentRecordEntry>(), true);
        }
    }

    // The table is created by supabase/student_records.sql; until it is run, show a hint instead of crashing.
    private static bool IsMissingTable(SupabaseException ex) =>
        ex.Code == "PGRST205" || ex.Code == "42P01" || ex.Message.Contains("Supabase tables are missing", StringComparison.OrdinalIgnoreCase);
}
