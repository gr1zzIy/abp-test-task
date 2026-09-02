using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Integration.Tests.Infrastructure;

/// <summary>
/// Запускає Web API з конфігурацією, ізольованою
/// від локального середовища розробника.
/// </summary>
public sealed class ConferenceBookingWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public ConferenceBookingWebApplicationFactory(
        string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        /*
         * Конфігурація додається до запуску entry point,
         * оскільки AddInfrastructure читає connection string
         * ще під час побудови застосунку.
         */
        builder.ConfigureHostConfiguration(configuration =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString,
                    ["Database:ApplyMigrationsOnStartup"] = "true",
                    ["Booking:TimeZone"] = "Europe/Kyiv",

                    ["Jwt:Issuer"] = "ConferenceBooking.IntegrationTests",
                    ["Jwt:Audience"] = "ConferenceBooking.IntegrationTests",
                    ["Jwt:Key"] = "integration-tests-only-signing-key-12345678901234567890",
                    ["Jwt:ExpirationMinutes"] = "60",

                    // Значення потрібні для проходження валідації
                    ["Admin:SeedOnStartup"] = "true",
                    ["Admin:Email"] = "admin@integration.test",
                    ["Admin:Password"] = "AdminPassword123"
                });
        });

        return base.CreateHost(builder);
    }
}