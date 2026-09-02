using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Integration.Tests.Infrastructure;

/// <summary>
/// Керує PostgreSQL-контейнером і тестовим Web API
/// протягом виконання integration tests.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("conference_booking_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

    public ConferenceBookingWebApplicationFactory Factory
    { get; private set; } = null!;

    public HttpClient Client
    { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        Factory = new ConferenceBookingWebApplicationFactory(
            _postgresContainer.GetConnectionString());

        /*
         * CreateClient запускає application host.
         * Під час startup застосуються реальні EF Core migrations
         * до PostgreSQL у Testcontainers.
         */
        Client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        await Factory.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}