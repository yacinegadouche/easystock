using EasyStock.Web.Data;
using EasyStock.Web.Models;
using EasyStock.Web.ViewModels;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EasyStock.Web.Services;

public class UserService(ApplicationDbContext db, IPasswordHasher<User> passwordHasher)
{
    public Task<List<UserListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        db.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new UserListItemViewModel(u.Id, u.Username, u.FullName, u.Role, u.IsActive))
            .ToListAsync(cancellationToken);

    public Task<UserEditViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default) =>
        db.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserEditViewModel
            {
                Id = u.Id,
                Username = u.Username,
                FullName = u.FullName,
                Role = u.Role,
                IsActive = u.IsActive,
            })
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ServiceResult> CreateAsync(UserCreateViewModel model, CancellationToken cancellationToken = default)
    {
        var username = model.Username.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Username == username, cancellationToken))
        {
            return ServiceResult.Failure($"The username '{username}' is already taken.");
        }

        var user = new User
        {
            Username = username,
            FullName = model.FullName.Trim(),
            Role = model.Role,
            PasswordHash = "",
        };
        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(
        UserEditViewModel model,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([model.Id], cancellationToken);

        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        if (user.Id == currentUserId && (!model.IsActive || model.Role != UserRole.Admin))
        {
            return ServiceResult.Failure("You cannot deactivate your own account or remove your own admin role.");
        }

        user.FullName = model.FullName.Trim();
        user.Role = model.Role;
        user.IsActive = model.IsActive;

        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ResetPasswordAsync(
        int id,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.PasswordHash = passwordHasher.HashPassword(user, newPassword);
        await db.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}