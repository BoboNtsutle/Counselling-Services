using System.Text.Json.Serialization;

namespace Ancora.Web.Models;

/// <summary>One row of "clinical_notes". Only admin and the assigned psychologist can read these (enforced by Supabase RLS).</summary>
public class ClinicalNote
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("appointment_id")] public Guid AppointmentId { get; set; }
    [JsonPropertyName("notes")] public string Notes { get; set; } = "";
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
}
