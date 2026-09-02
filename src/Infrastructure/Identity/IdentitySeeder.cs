using Application.Common.Security;
using Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity;

/// <summary>
/// Створює початковий обліковий запис адміністратора,
/// якщо це явно дозволено конфігурацією.
/// </summary>
internal sealed class IdentitySeeder : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<IdentitySeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = _configuration
            .GetSection(AdminOptions.SectionName)
            .Get<AdminOptions>();

        if (options?.SeedOnStartup != true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Admin seed is enabled, but admin credentials are not configured.");
        }

        using var scope = _scopeFactory.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var existingUser = await userManager.FindByEmailAsync(
            options.Email);

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
            UserName = options.Email,
            Email = options.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(
            user,
            options.Password);

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
            options.Email);
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