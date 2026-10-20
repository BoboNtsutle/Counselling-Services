using System.Text.Json.Serialization;

namespace Ancora.Web.Models;

/// <summary>One row of the Supabase "profiles" table (same table the Flutter app uses).</summary>
public class Profile
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("display_name")] public string DisplayName { get; set; } = "";
    [JsonPropertyName("role")] public string Role { get; set; } = Roles.Student;
    [JsonPropertyName("requested_role")] public string RequestedRole { get; set; } = Roles.Student;

    /// <summary>none / pending / approved / rejected</summary>
    [JsonPropertyName("role_request_status")] public string RoleRequestStatus { get; set; } = "none";

    [JsonPropertyName("student_number")] public string StudentNumber { get; set; } = "";
    [JsonPropertyName("course")] public string Course { get; set; } = "";
    [JsonPropertyName("phone_number")] public string PhoneNumber { get; set; } = "";
    [JsonPropertyName("bio")] public string Bio { get; set; } = "";
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}
