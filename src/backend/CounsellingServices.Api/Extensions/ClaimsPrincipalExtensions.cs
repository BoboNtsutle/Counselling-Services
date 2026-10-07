using System.Security.Claims;

namespace CounsellingServices.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        if (Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var parsed))
        {
            return parsed;
        }

        if (Guid.TryParse(principal.FindFirstValue("sub"), out parsed))
        {
            return parsed;
        }

        throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
    }
}
