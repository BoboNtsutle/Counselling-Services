using Ancora.Web.Models;
using Ancora.Web.Models.ViewModels;
using Ancora.Web.Services.Supabase;

namespace Ancora.Web.Services;

/// <summary>
/// MODULE 1 — Foundation, Login &amp; My Profile (FR-01, FR-10)
/// Owner: see TEAM-PLAN.md
/// </summary>
public class ProfileService
{
    private readonly SupabaseClient _db;

    public ProfileService(SupabaseClient db) => _db = db;

    /// <summary>Loads a profile row. accessToken is only passed during login (before the cookie exists).</summary>
    public Task<Profile?> GetProfileAsync(Guid userId, string? accessToken = null) =>
        _db.GetSingleAsync<Profile>("profiles", $"id=eq.{userId}", accessToken);

    /// <summary>Appointments for a given student.</summary>
    public Task<List<Appointment>> GetMyAppointmentsAsync(Guid studentId) =>
        _db.GetAsync<Appointment>("appointments", $"student_id=eq.{studentId}&order=appointment_date,appointment_time");

    /// <summary>
    /// Saves a new appointment request for a student. It starts as "Pending";
    /// the admin later approves it and assigns a psychologist.
    /// </summary>
    public Task BookAppointmentAsync(Profile student, BookSessionViewModel form)
    {
        // The "reason" column holds the category, plus the student's own words if they wrote any.
        var reason = form.ProblemCategory ?? "";
        if (!string.IsNullOrWhiteSpace(form.Description))
            reason += ": " + form.Description.Trim();

        return _db.InsertAsync<Appointment>("appointments", new
        {
            student_id = student.Id,
            student_name = student.DisplayName,
            student_number = student.StudentNumber,
            course = student.Course,
            appointment_date = form.AppointmentDate!.Value.ToString("yyyy-MM-dd"),
            appointment_time = form.AppointmentTime,          // already "08:00" style text
            reason,
            mood = form.Mood ?? "",
            urgency = form.Urgency,
            status = AppointmentStatus.Pending,
            created_at = DateTimeOffset.UtcNow
        });
    }

    /// <summary>Cancel an appointment by the student (sets status = Rejected).</summary>
    public Task CancelAppointmentAsync(Guid appointmentId, Guid studentId)
    {
        // Ensure the caller is the student who owns the appointment
        return _db.UpdateAsync("appointments", $"id=eq.{appointmentId}&student_id=eq.{studentId}", new { status = AppointmentStatus.Rejected });
    }

    // TODO (Module 1): let a user update their own display_name and phone_number.
    // Hint:
    //   await _db.UpdateAsync("profiles", $"id=eq.{userId}",
    //       new { display_name = displayName.Trim(), phone_number = phone.Trim() });
    // The database blocks users from changing their own role, so you only send these two fields.
    public Task UpdateMyProfileAsync(Guid userId, string displayName, string phoneNumber, string bio) =>
        _db.UpdateAsync("profiles", $"id=eq.{userId}", new { display_name = displayName.Trim(), phone_number = phoneNumber.Trim(), bio = bio.Trim() });
}
