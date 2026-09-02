using System.Net;
using Infrastructure.Persistence;
using Integration.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Tests.Smoke;

[Collection(IntegrationTestCollection.Name)]
public sealed class ApplicationSmokeTests
{
    private readonly IntegrationTestFixture _fixture;

    public ApplicationSmokeTests(
        IntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Ping_ReturnsOk()
    {
        var response = await _fixture.Client.GetAsync(
            "/api/ping");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Database_AfterStartup_ContainsSeededServices()
    {
        using var scope =
            _fixture.Factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var servicesCount = await dbContext.Services
            .CountAsync();

        Assert.Equal(3, servicesCount);
    }
}