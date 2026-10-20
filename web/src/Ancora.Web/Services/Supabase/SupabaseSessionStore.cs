using System.Security.Claims;
using Ancora.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Ancora.Web.Services.Supabase;

/// <summary>
/// Keeps the logged-in user's Supabase tokens inside the (encrypted) login cookie,
/// and refreshes the access token when it is about to expire (Supabase tokens last ~1 hour).
///
/// You normally do NOT use this class directly — SupabaseClient calls it for you.
/// </summary>
public class SupabaseSessionStore
{
    public const string AccessTokenClaim = "sb_access_token";
    public const string RefreshTokenClaim = "sb_refresh_token";
    public const string ExpiresAtClaim = "sb_expires_at";

    private readonly IHttpContextAccessor _accessor;
    private readonly SupabaseAuthService _auth;
    private readonly ILogger<SupabaseSessionStore> _logger;
    private string? _refreshedToken; // cache for the rest of this request after a refresh

    public SupabaseSessionStore(IHttpContextAccessor accessor, SupabaseAuthService auth, ILogger<SupabaseSessionStore> logger)
    {
        _accessor = accessor;
        _auth = auth;
        _logger = logger;
    }

    /// <summary>Signs the user into the website (writes the cookie).</summary>
    public async Task SignInAsync(SupabaseSession session, Profile profile)
    {
        var context = _accessor.HttpContext ?? throw new InvalidOperationException("No HTTP context.");
        var identity = new ClaimsIdentity(BuildClaims(session, profile), CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });
    }

    /// <summary>Returns a valid access token for the current user, refreshing it if needed.</summary>
    public async Task<string?> GetAccessTokenAsync()
    {
        if (_refreshedToken != null) return _refreshedToken;

        var user = _accessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return null;

        var token = user.FindFirstValue(AccessTokenClaim);
        var refresh = user.FindFirstValue(RefreshTokenClaim);
        long.TryParse(user.FindFirstValue(ExpiresAtClaim), out var expiresAt);

        var expiresSoon = DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresAt - 60;
        if (!expiresSoon || string.IsNullOrEmpty(refresh)) return token;

        try
        {
            var session = await _auth.RefreshAsync(refresh);
            var profile = new Profile
            {
                Id = session.User.Id,
                Email = user.FindFirstValue(ClaimTypes.Email) ?? "",
                DisplayName = user.Identity?.Name ?? "",
                Role = user.FindFirstValue(ClaimTypes.Role) ?? Roles.Student,
            };
            await SignInAsync(session, profile);
            _refreshedToken = session.AccessToken;
            return session.AccessToken;
        }
        catch (SupabaseException ex)
        {
            _logger.LogWarning(ex, "Could not refresh the Supabase session.");
            return token; // the request will fail with 401 and the user gets sent to login
        }
    }

    private static IEnumerable<Claim> BuildClaims(SupabaseSession session, Profile profile) => new[]
    {
        new Claim(ClaimTypes.NameIdentifier, profile.Id.ToString()),
        new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(profile.DisplayName) ? profile.Email : profile.DisplayName),
        new Claim(ClaimTypes.Email, profile.Email),
        new Claim(ClaimTypes.Role, profile.Role),
        new Claim(AccessTokenClaim, session.AccessToken),
        new Claim(RefreshTokenClaim, session.RefreshToken),
        new Claim(ExpiresAtClaim, session.ExpiresAt.ToString()),
    };
}
