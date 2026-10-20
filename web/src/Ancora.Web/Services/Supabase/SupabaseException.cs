using System.Net;
using System.Text.Json;

namespace Ancora.Web.Services.Supabase;

/// <summary>Thrown when Supabase returns an error. Message is already user-friendly.</summary>
public class SupabaseException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Code { get; }

    public SupabaseException(string message, HttpStatusCode statusCode, string? code = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
    }

    /// <summary>Turns a raw Supabase error body into a readable message (same wording as the Flutter app).</summary>
    public static SupabaseException From(HttpStatusCode status, string body)
    {
        string? code = null;
        var raw = body;
        try
        {
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;
            foreach (var key in new[] { "msg", "message", "error_description", "error" })
            {
                if (root.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    raw = value.GetString() ?? body;
                    break;
                }
            }
            if (root.TryGetProperty("code", out var c)) code = c.ToString();
        }
        catch (JsonException)
        {
            // body was not JSON; keep it as is
        }

        var lower = raw.ToLowerInvariant();
        var friendly =
            lower.Contains("invalid login credentials") ? "Email or password is incorrect." :
            lower.Contains("email not confirmed") ? "Confirm your email address first, then sign in." :
            code == "42501" ? "You are not allowed to do this (blocked by Supabase row-level security)." :
            code == "PGRST205" || lower.Contains("schema cache") ? "Supabase tables are missing. Run supabase/schema.sql in the SQL Editor." :
            status == HttpStatusCode.Unauthorized ? "Your session has expired. Please sign in again." :
            raw;

        return new SupabaseException(friendly, status, code);
    }
}
