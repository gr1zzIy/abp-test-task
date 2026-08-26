using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Extensions;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Застосовує незастосовані міграції БД, якщо це явно
    /// дозволено конфігурацією застосунку.
    /// </summary>
    public static async Task ApplyDatabaseMigrationsAsync(
        this WebApplication app)
    {
        var applyMigrations = app.Configuration
            .GetValue<bool>("Database:ApplyMigrationsOnStartup");

        if (!applyMigrations)
        {
            return;
        }

        await using var scope = app.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILogger<AppDbContext>>();

        logger.LogInformation("Applying database migrations.");

        await dbContext.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully.");
    }
}