using Ancora.Web.Models;
using Ancora.Web.Services.Supabase;

namespace Ancora.Web.Services;

/// <summary>
/// MODULE 2 — Admin: Appointment management + double-booking guard (FR-03, FR-04)
/// Owner: see TEAM-PLAN.md
/// </summary>
public class AppointmentAdminService
{
    private readonly SupabaseClient _db;

    public AppointmentAdminService(SupabaseClient db) => _db = db;

    /// <summary>All appointments, newest first. Pass a status to filter (Pending, Approved, ...).</summary>
    public Task<List<Appointment>> GetAllAsync(string? status = null)
    {
        var query = "order=created_at.desc";
        if (!string.IsNullOrWhiteSpace(status)) query = $"status=eq.{Uri.EscapeDataString(status)}&{query}";
        return _db.GetAsync<Appointment>("appointments", query);
    }

    public Task<Appointment?> GetByIdAsync(Guid id) =>
        _db.GetSingleAsync<Appointment>("appointments", $"id=eq.{id}");

    /// <summary>Psychologists for the "assign to" dropdown.</summary>
    public Task<List<Profile>> GetPsychologistsAsync() =>
        _db.GetAsync<Profile>("profiles", $"role=eq.{Roles.Psychologist}&order=display_name");

    // TODO (Module 2): FR-04 double-booking guard.
    // Return true if the psychologist already has an APPROVED appointment on the same date + time.
    // Hint:
    //   var clashes = await _db.GetAsync<Appointment>("appointments",
    //       $"psychologist_id=eq.{psychologistId}&status=eq.Approved" +
    //       $"&appointment_date=eq.{Uri.EscapeDataString(date)}" +
    //       $"&appointment_time=eq.{Uri.EscapeDataString(time)}" +
    //       $"&id=neq.{excludeAppointmentId}");
    //   return clashes.Count > 0;
    public async Task<bool> HasClashAsync(Guid psychologistId, string date, string time, Guid excludeAppointmentId)
    {
        // Return true if the psychologist already has an APPROVED appointment on the same date + time.
        var clashes = await _db.GetAsync<Appointment>("appointments",
            $"psychologist_id=eq.{psychologistId}&status=eq.{AppointmentStatus.Approved}" +
            $"&appointment_date=eq.{Uri.EscapeDataString(date)}" +
            $"&appointment_time=eq.{Uri.EscapeDataString(time)}" +
            $"&id=neq.{excludeAppointmentId}");
        return clashes.Count > 0;
    }

    // TODO (Module 2): assign a psychologist and approve.
    // 1. Load the appointment.  2. If HasClashAsync(...) → throw/return an error message.
    // 3. await _db.UpdateAsync("appointments", $"id=eq.{id}",
    //        new { psychologist_id = psychologistId, status = AppointmentStatus.Approved });
    // (The database trigger fills in psychologist_name for you.)
    public async Task AssignAndApproveAsync(Guid appointmentId, Guid psychologistId)
    {
        var ap = await GetByIdAsync(appointmentId);
        if (ap == null) throw new InvalidOperationException("Appointment not found.");

        var date = ap.AppointmentDate;
        var time = ap.AppointmentTime;
        if (await HasClashAsync(psychologistId, date, time, appointmentId))
            throw new InvalidOperationException("That psychologist is already booked for that slot.");

        await _db.UpdateAsync("appointments", $"id=eq.{appointmentId}",
            new { psychologist_id = psychologistId, status = AppointmentStatus.Approved });
    }

    // TODO (Module 2): reject a request → status = Rejected.
    public Task RejectAsync(Guid appointmentId) =>
        _db.UpdateAsync("appointments", $"id=eq.{appointmentId}", new { status = AppointmentStatus.Rejected });

    // TODO (Module 2): reschedule → change appointment_date / appointment_time
    // (run HasClashAsync first if a psychologist is already assigned).
    public async Task RescheduleAsync(Guid appointmentId, string newDate, string newTime)
    {
        var ap = await GetByIdAsync(appointmentId);
        if (ap == null) throw new InvalidOperationException("Appointment not found.");

        if (ap.PsychologistId != null)
        {
            if (await HasClashAsync(ap.PsychologistId.Value, newDate, newTime, appointmentId))
                throw new InvalidOperationException("Reschedule would cause a double booking for the assigned psychologist.");
        }

        await _db.UpdateAsync("appointments", $"id=eq.{appointmentId}",
            new { appointment_date = newDate, appointment_time = newTime });
    }
}
