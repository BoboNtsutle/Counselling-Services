using Ancora.Web.Models;
using Ancora.Web.Services.Supabase;

namespace Ancora.Web.Services;

/// <summary>
/// MODULE 5 — Psychologist portal: my appointments, status updates, session notes (FR-05, FR-09)
/// Owner: see TEAM-PLAN.md
/// </summary>
public class PsychologistService
{
    private readonly SupabaseClient _db;

    public PsychologistService(SupabaseClient db) => _db = db;

    /// <summary>Appointments assigned to this psychologist (Supabase RLS also enforces this).</summary>
    public Task<List<Appointment>> GetMyAppointmentsAsync(Guid psychologistId) =>
        _db.GetAsync<Appointment>("appointments", $"psychologist_id=eq.{psychologistId}&order=created_at.desc");

    public Task<Appointment?> GetMyAppointmentAsync(Guid psychologistId, Guid appointmentId) =>
        _db.GetSingleAsync<Appointment>("appointments", $"id=eq.{appointmentId}&psychologist_id=eq.{psychologistId}");

    // TODO (Module 5): FR-09 update status. The database only lets a psychologist change
    // the status, and only to Approved, Rejected or Complete (see guard_appointment_update in schema.sql).
    //   await _db.UpdateAsync("appointments", $"id=eq.{appointmentId}", new { status = newStatus });
    public async Task UpdateStatusAsync(Guid appointmentId, Guid psychologistId, string newStatus)
    {
        // Load the appointment to validate current state and ownership
        var ap = await _db.GetSingleAsync<Appointment>("appointments", $"id=eq.{appointmentId}");
        if (ap == null) throw new InvalidOperationException("Appointment not found.");
        if (ap.PsychologistId == null || ap.PsychologistId != psychologistId)
            throw new InvalidOperationException("You are not assigned to this appointment.");

        // Enforce allowed transitions:
        // Pending  -> Approved, Rejected
        // Approved -> Complete
        // Complete/Rejected -> no further changes
        var cur = ap.Status ?? "";
        var allowed = cur switch
        {
            AppointmentStatus.Pending => new[] { AppointmentStatus.Approved, AppointmentStatus.Rejected },
            AppointmentStatus.Approved => new[] { AppointmentStatus.Complete },
            _ => Array.Empty<string>()
        };

        if (!allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot change status from {cur} to {newStatus}.");

        await _db.UpdateAsync("appointments", $"id=eq.{appointmentId}", new { status = newStatus });
    }

    // TODO (Module 5): load session notes (empty string if none yet).
    //   var note = await _db.GetSingleAsync<ClinicalNote>("clinical_notes", $"appointment_id=eq.{appointmentId}");
    //   return note?.Notes ?? "";
    public Task<string> GetNotesAsync(Guid appointmentId) =>
        _db.GetSingleAsync<ClinicalNote>("clinical_notes", $"appointment_id=eq.{appointmentId}")
            .ContinueWith(t => t.Result?.Notes ?? "");

    // TODO (Module 5): save session notes (insert or update — one note row per appointment).
    //   await _db.UpsertAsync("clinical_notes",
    //       new { appointment_id = appointmentId, notes, updated_at = DateTimeOffset.UtcNow },
    //       onConflict: "appointment_id");
    public Task SaveNotesAsync(Guid appointmentId, string notes) =>
        _db.UpsertAsync("clinical_notes", new { appointment_id = appointmentId, notes, updated_at = DateTimeOffset.UtcNow }, onConflict: "appointment_id");
}
