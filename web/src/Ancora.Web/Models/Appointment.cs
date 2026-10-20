using System.Text.Json.Serialization;

namespace Ancora.Web.Models;

/// <summary>One row of the Supabase "appointments" table.</summary>
/// <remarks>
/// appointment_date and appointment_time are TEXT columns. The Flutter app currently
/// saves them as display text (e.g. "Fri, Oct 9" and "9:00 AM"), so treat them as text
/// until the group agrees on a format (see TEAM-PLAN.md, "Decisions to make together").
/// </remarks>
public class Appointment
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("student_id")] public Guid StudentId { get; set; }
    [JsonPropertyName("psychologist_id")] public Guid? PsychologistId { get; set; }
    [JsonPropertyName("psychologist_name")] public string PsychologistName { get; set; } = "";
    [JsonPropertyName("student_name")] public string StudentName { get; set; } = "";
    [JsonPropertyName("student_number")] public string StudentNumber { get; set; } = "";
    [JsonPropertyName("course")] public string Course { get; set; } = "";
    [JsonPropertyName("appointment_date")] public string AppointmentDate { get; set; } = "";
    [JsonPropertyName("appointment_time")] public string AppointmentTime { get; set; } = "";
    [JsonPropertyName("reason")] public string Reason { get; set; } = "";
    [JsonPropertyName("mood")] public string Mood { get; set; } = "";
    [JsonPropertyName("urgency")] public string Urgency { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = AppointmentStatus.Pending;
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
}
