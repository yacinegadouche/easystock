using System.Security.Claims;

namespace EasyStock.Web.Authorization;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("The user id claim is missing."));
}