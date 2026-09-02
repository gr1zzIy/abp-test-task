using System.Net;
using System.Net.Http.Json;
using Application.Authentication.Me;
using Integration.Tests.Helpers;
using Integration.Tests.Infrastructure;

namespace Integration.Tests.Authentication;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthenticationFlowTests
{
    private readonly HttpClient _client;

    public AuthenticationFlowTests(
        IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task RegisterAndLogin_ValidCredentials_ReturnsAuthenticatedUser()
    {
        var email =
            $"client-{Guid.NewGuid():N}@integration.test";

        var registeredUser =
            await AuthenticationTestHelper.RegisterAsync(
                _client,
                email);

        var accessToken =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                email);

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                "/api/auth/me")
                .WithBearerToken(accessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var currentUser = await response.Content
            .ReadFromJsonAsync<CurrentUserResult>();

        Assert.NotNull(currentUser);
        Assert.Equal(registeredUser.Id, currentUser.Id);
        Assert.Equal(email, currentUser.Email);
        Assert.Contains("Client", currentUser.Roles);
    }

    [Fact]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var email =
            $"client-{Guid.NewGuid():N}@integration.test";

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            email);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = email,
                Password = "WrongPassword123"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task Me_WithoutAccessToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync(
            "/api/auth/me");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
    
    [Fact]
    public async Task Reports_ClientToken_ReturnsForbidden()
    {
        var email =
            $"client-{Guid.NewGuid():N}@integration.test";

        await AuthenticationTestHelper.RegisterAsync(
            _client,
            email);

        var token =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                email);

        using var request =
            new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/reports/revenue?from=2030-01-01T00:00:00Z&to=2030-02-01T00:00:00Z")
                .WithBearerToken(token);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
    
    [Fact]
    public async Task Reports_AdminToken_ReturnsOk()
    {
        var token =
            await AuthenticationTestHelper.LoginAsync(
                _client,
                "admin@integration.test",
                "AdminPassword123");

        using var request =
            new HttpRequestMessage(
                    HttpMethod.Get,
                    "/api/reports/revenue?from=2030-01-01T00:00:00Z&to=2030-02-01T00:00:00Z")
                .WithBearerToken(token);

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}