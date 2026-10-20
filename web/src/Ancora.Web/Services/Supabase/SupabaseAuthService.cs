using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Ancora.Web.Services.Supabase;

/// <summary>What Supabase Auth gives back after a successful sign-in.</summary>
public class SupabaseSession
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
    [JsonPropertyName("refresh_token")] public string RefreshToken { get; set; } = "";
    [JsonPropertyName("expires_at")] public long ExpiresAt { get; set; }   // unix seconds
    [JsonPropertyName("user")] public SupabaseUser User { get; set; } = new();
}

public class SupabaseUser
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; } = "";
}

/// <summary>
/// Talks to Supabase Auth (GoTrue) at {Url}/auth/v1.
/// Same accounts as the Flutter app — a psychologist can log into both with one password.
/// </summary>
public class SupabaseAuthService
{
    private readonly HttpClient _http;
    private readonly SupabaseOptions _options;

    public SupabaseAuthService(HttpClient http, IOptions<SupabaseOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public Task<SupabaseSession> SignInAsync(string email, string password) =>
        PostForSessionAsync("auth/v1/token?grant_type=password", new { email = email.Trim(), password });

    public Task<SupabaseSession> SignUpAsync(string email, string password) =>
        PostForSessionAsync("auth/v1/signup", new { email = email.Trim(), password });

    public Task<SupabaseSession> RefreshAsync(string refreshToken) =>
        PostForSessionAsync("auth/v1/token?grant_type=refresh_token", new { refresh_token = refreshToken });

    /// <summary>Sends the "reset your password" email.</summary>
    public async Task SendPasswordResetAsync(string email)
    {
        using var request = NewRequest(HttpMethod.Post, "auth/v1/recover");
        request.Content = ToJson(new { email = email.Trim() });
        using var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            throw SupabaseException.From(response.StatusCode, await response.Content.ReadAsStringAsync());
    }

    /// <summary>Ends the session on Supabase's side too (best effort).</summary>
    public async Task SignOutAsync(string accessToken)
    {
        using var request = NewRequest(HttpMethod.Post, "auth/v1/logout");
        request.Headers.Authorization = new("Bearer", accessToken);
        try { using var _ = await _http.SendAsync(request); } catch (HttpRequestException) { }
    }

    private async Task<SupabaseSession> PostForSessionAsync(string path, object body)
    {
        using var request = NewRequest(HttpMethod.Post, path);
        request.Content = ToJson(body);
        using var response = await _http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode) throw SupabaseException.From(response.StatusCode, text);
        return JsonSerializer.Deserialize<SupabaseSession>(text)
               ?? throw new SupabaseException("Sign-in failed. Try again.", response.StatusCode);
    }

    private static StringContent ToJson(object body) => SupabaseClient.ToJson(body);

    private HttpRequestMessage NewRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, $"{_options.Url.TrimEnd('/')}/{path}");
        request.Headers.Add("apikey", _options.PublishableKey);
        return request;
    }
}
