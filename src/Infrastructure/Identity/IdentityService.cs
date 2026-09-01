using Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

internal sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<CreateUserResult> CreateUserAsync(
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email
        };

        var result = await _userManager.CreateAsync(
            user,
            password);

        if (!result.Succeeded)
        {
            return new CreateUserResult(
                false,
                null,
                result.Errors
                    .Select(error => error.Description)
                    .ToArray());
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            role);

        if (!roleResult.Succeeded)
        {
            // Користувач без ролі в системі не повинен залишатися.
            await _userManager.DeleteAsync(user);

            return new CreateUserResult(
                false,
                null,
                roleResult.Errors
                    .Select(error => error.Description)
                    .ToArray());
        }

        return new CreateUserResult(
            true,
            user.Id,
            []);
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return null;
        }

        var passwordValid = await _userManager.CheckPasswordAsync(
            user,
            password);

        if (!passwordValid)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new AuthenticatedUser(
            user.Id,
            user.Email!,
            roles.ToArray());
    }
}