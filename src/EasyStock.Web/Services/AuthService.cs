using System.Security.Claims;
using EasyStock.Web.Data;
using EasyStock.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class AuthService(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
{
    public const string FullNameClaimType = "full_name";

    public async Task<User?> ValidateCredentialsAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        var user = await db.Users
            .SingleOrDefaultAsync(u => u.Username == normalizedUsername, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, password);
            await db.SaveChangesAsync(cancellationToken);
        }

        return user;
    }

    public static ClaimsPrincipal CreatePrincipal(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(FullNameClaimType, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
        ];

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}