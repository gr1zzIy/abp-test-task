using Application.Common.Security;
using Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Identity;

/// <summary>
/// Створює початковий обліковий запис адміністратора,
/// якщо це явно дозволено конфігурацією.
/// </summary>
internal sealed class IdentitySeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IdentitySeeder> _logger;
    private readonly AdminOptions _options;

    public IdentitySeeder(
        IServiceScopeFactory scopeFactory,
        IOptions<AdminOptions> options,
        ILogger<IdentitySeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.SeedOnStartup)
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var existingUser = await userManager.FindByEmailAsync(
            _options.Email);

        if (existingUser is not null)
        {
            var isAdmin = await userManager.IsInRoleAsync(
                existingUser,
                Roles.Admin);

            if (!isAdmin)
            {
                // Не підвищуємо автоматично існуючого користувача до Admin:
                // email міг бути зареєстрований через публічний endpoint раніше.
                throw new InvalidOperationException(
                    "Configured admin email already belongs to a non-admin user.");
            }

            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = _options.Email,
            Email = _options.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(
            user,
            _options.Password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Unable to create admin user: {FormatErrors(createResult)}");
        }

        var roleResult = await userManager.AddToRoleAsync(
            user,
            Roles.Admin);

        if (!roleResult.Succeeded)
        {
            // Не залишаємо користувача в стані без ролі.
            await userManager.DeleteAsync(user);

            throw new InvalidOperationException(
                $"Unable to assign Admin role: {FormatErrors(roleResult)}");
        }

        _logger.LogInformation(
            "Initial administrator account was created for {Email}.",
            _options.Email);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static string FormatErrors(IdentityResult result)
    {
        return string.Join(
            "; ",
            result.Errors.Select(error => error.Description));
    }
}