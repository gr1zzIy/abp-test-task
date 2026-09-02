using System.Net;
using Integration.Tests.Infrastructure;

namespace Integration.Tests.Health;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthCheckTests
{
    private readonly HttpClient _client;

    public HealthCheckTests(
        IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Live_ApplicationIsRunning_ReturnsOk()
    {
        var response = await _client.GetAsync(
            "/health/live");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task Ready_DatabaseIsAvailable_ReturnsOk()
    {
        var response = await _client.GetAsync(
            "/health/ready");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}