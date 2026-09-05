using System.Security.Claims;

namespace AtelieBebe.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Token sem identificador de usuário.");
        return Guid.Parse(value);
    }

    /// <summary>Like <see cref="GetUserId"/>, but returns null instead of throwing when there is no authenticated user.</summary>
    public static Guid? GetUserIdOrNull(this ClaimsPrincipal principal) =>
        principal.Identity?.IsAuthenticated == true ? principal.GetUserId() : null;
}
