using System.Text.Json.Serialization;

namespace Ancora.Web.Models;

/// <summary>One row of "announcements". Students see these in the Flutter app.</summary>
public class Announcement
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("message")] public string Message { get; set; } = "";
    [JsonPropertyName("image_url")] public string? ImageUrl { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
}
