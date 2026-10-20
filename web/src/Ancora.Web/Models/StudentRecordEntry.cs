using System.Text.Json.Serialization;

namespace Ancora.Web.Models;

/// <summary>
/// One saved entry in a student's permanent record (table "student_records").
/// Entries are written by psychologists and are never deleted from the web app, so the
/// record keeps growing. student_name / student_number are copied in when the entry is saved,
/// so an entry stays identifiable even if the student later changes their profile.
/// </summary>
public class StudentRecordEntry
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("student_id")] public Guid StudentId { get; set; }
    [JsonPropertyName("student_name")] public string StudentName { get; set; } = "";
    [JsonPropertyName("student_number")] public string StudentNumber { get; set; } = "";
    [JsonPropertyName("appointment_id")] public Guid? AppointmentId { get; set; }
    [JsonPropertyName("author_id")] public Guid AuthorId { get; set; }
    [JsonPropertyName("author_name")] public string AuthorName { get; set; } = "";
    [JsonPropertyName("category")] public string Category { get; set; } = StudentRecordCategories.General;
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("notes")] public string Notes { get; set; } = "";
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>True when the entry was changed after it was first saved.</summary>
    [JsonIgnore] public bool WasEdited => (UpdatedAt - CreatedAt).TotalSeconds > 5;
}

/// <summary>The kinds of entry a psychologist can file on a student's record.</summary>
public static class StudentRecordCategories
{
    public const string General = "General note";
    public static readonly string[] All =
    {
        "General note", "Session summary", "Assessment", "Progress note", "Follow-up plan", "Referral"
    };
}
