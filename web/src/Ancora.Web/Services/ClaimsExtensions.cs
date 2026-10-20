using System.Security.Claims;

namespace Ancora.Web.Services;

public static class ClaimsExtensions
{
    /// <summary>The logged-in user's Supabase id (same as profiles.id). Usage: User.GetUserId()</summary>
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("Not signed in."));
}
