using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Ancora.Web.Services.Supabase;

/// <summary>
/// The ONE class everybody uses to read and write Supabase tables.
///
/// Every request is sent AS THE LOGGED-IN USER (their access token), so the
/// row-level security rules in supabase/schema.sql still apply:
///   - a psychologist only gets their own appointments,
///   - only an admin can change roles or publish announcements, etc.
/// That is why the web app and the Flutter app always follow the same rules.
///
/// Queries use PostgREST syntax, e.g.
///   await _db.GetAsync&lt;Appointment&gt;("appointments", "status=eq.Pending&amp;order=created_at.desc");
/// Cheat sheet: https://postgrest.org/en/stable/references/api/tables_views.html
///   eq. = equals, neq. = not equal, ilike.*text* = contains (case-insensitive),
///   in.(a,b) = one of, is.null, order=col.desc, limit=10, select=col1,col2
/// </summary>
public class SupabaseClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly SupabaseOptions _options;
    private readonly SupabaseSessionStore _session;

    public SupabaseClient(HttpClient http, IOptions<SupabaseOptions> options, SupabaseSessionStore session)
    {
        _http = http;
        _options = options.Value;
        _session = session;
    }

    // ---------- Tables (PostgREST at /rest/v1) ----------

    /// <summary>SELECT rows. Example: GetAsync&lt;Profile&gt;("profiles", "role=eq.psychologist&amp;order=display_name")</summary>
    public async Task<List<T>> GetAsync<T>(string table, string query = "", string? accessToken = null)
    {
        using var request = await NewRequestAsync(HttpMethod.Get, Rest(table, query), accessToken);
        return await SendAsync<List<T>>(request) ?? new List<T>();
    }

    /// <summary>SELECT one row, or null if nothing matches.</summary>
    public async Task<T?> GetSingleAsync<T>(string table, string query, string? accessToken = null)
    {
        var rows = await GetAsync<T>(table, AddLimit(query), accessToken);
        return rows.FirstOrDefault();
    }

    /// <summary>
    /// Counts rows without downloading them. Example: CountAsync("appointments", "status=eq.Pending")
    /// </summary>
    public async Task<int> CountAsync(string table, string query = "")
    {
        var q = string.IsNullOrEmpty(query) ? "select=id" : $"select=id&{query}";
        using var request = await NewRequestAsync(HttpMethod.Head, Rest(table, q));
        request.Headers.Add("Prefer", "count=exact");
        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw SupabaseException.From(response.StatusCode, await response.Content.ReadAsStringAsync());
        // Content-Range looks like "0-24/57" (or "*/0"); the number after "/" is the total.
        var range = response.Content.Headers.TryGetValues("Content-Range", out var values) ? values.First() : "*/0";
        return int.TryParse(range.Split('/').Last(), out var total) ? total : 0;
    }

    /// <summary>INSERT a row and return what was saved. Use an anonymous object with snake_case names.</summary>
    public async Task<T?> InsertAsync<T>(string table, object row)
    {
        using var request = await NewRequestAsync(HttpMethod.Post, Rest(table, ""));
        request.Headers.Add("Prefer", "return=representation");
        request.Content = ToJson(row);
        var rows = await SendAsync<List<T>>(request);
        return rows is { Count: > 0 } ? rows[0] : default;
    }

    /// <summary>
    /// UPDATE rows that match the filter. Example:
    ///   UpdateAsync("appointments", $"id=eq.{id}", new { status = "Approved" })
    /// </summary>
    public async Task UpdateAsync(string table, string filter, object changes)
    {
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("Refusing to update without a filter (it would change every row).");
        using var request = await NewRequestAsync(HttpMethod.Patch, Rest(table, filter));
        request.Headers.Add("Prefer", "return=minimal");
        request.Content = ToJson(changes);
        await SendAsync<object>(request, expectBody: false);
    }

    /// <summary>INSERT, or UPDATE if a row with the same onConflict column already exists.</summary>
    public async Task UpsertAsync(string table, object row, string onConflict)
    {
        using var request = await NewRequestAsync(HttpMethod.Post, Rest(table, $"on_conflict={onConflict}"));
        request.Headers.Add("Prefer", "resolution=merge-duplicates,return=minimal");
        request.Content = ToJson(row);
        await SendAsync<object>(request, expectBody: false);
    }

    /// <summary>DELETE rows that match the filter.</summary>
    public async Task DeleteAsync(string table, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            throw new ArgumentException("Refusing to delete without a filter (it would delete every row).");
        using var request = await NewRequestAsync(HttpMethod.Delete, Rest(table, filter));
        await SendAsync<object>(request, expectBody: false);
    }

    // ---------- Storage (/storage/v1) ----------

    /// <summary>Uploads a file to a storage bucket and returns its public URL.</summary>
    public async Task<string> UploadAsync(string bucket, string path, Stream content, string contentType)
    {
        using var request = await NewRequestAsync(HttpMethod.Post, $"{Base}/storage/v1/object/{bucket}/{path}");
        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        await SendAsync<object>(request, expectBody: false);
        return PublicUrl(bucket, path);
    }

    public string PublicUrl(string bucket, string path) => $"{Base}/storage/v1/object/public/{bucket}/{path}";

    // ---------- plumbing ----------

    private string Base => _options.Url.TrimEnd('/');

    /// <summary>Serialises a body to JSON with a fixed Content-Length (no chunked upload).</summary>
    internal static StringContent ToJson(object body) =>
        new(JsonSerializer.Serialize(body, Json), System.Text.Encoding.UTF8, "application/json");

    private string Rest(string table, string query) =>
        string.IsNullOrEmpty(query) ? $"{Base}/rest/v1/{table}" : $"{Base}/rest/v1/{table}?{query}";

    private static string AddLimit(string query) =>
        query.Contains("limit=") ? query : (string.IsNullOrEmpty(query) ? "limit=1" : $"{query}&limit=1");

    private async Task<HttpRequestMessage> NewRequestAsync(HttpMethod method, string url, string? accessToken = null)
    {
        var token = accessToken ?? await _session.GetAccessTokenAsync()
            ?? throw new SupabaseException("Please sign in first.", System.Net.HttpStatusCode.Unauthorized);
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("apikey", _options.PublishableKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<T?> SendAsync<T>(HttpRequestMessage request, bool expectBody = true)
    {
        using var response = await _http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw SupabaseException.From(response.StatusCode, text);
        if (!expectBody || string.IsNullOrWhiteSpace(text)) return default;
        return JsonSerializer.Deserialize<T>(text, Json);
    }
}
