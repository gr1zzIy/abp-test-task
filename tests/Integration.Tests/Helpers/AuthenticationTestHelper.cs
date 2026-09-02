using System.Net.Http.Json;
using Application.Authentication.Login;
using Application.Authentication.Register;

namespace Integration.Tests.Helpers;

public static class AuthenticationTestHelper
{
    public static async Task<RegisterResult> RegisterAsync(
        HttpClient client,
        string email,
        string password = "Password123")
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new
            {
                Email = email,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<RegisterResult>()
               ?? throw new InvalidOperationException(
                   "Registration response was empty.");
    }

    public static async Task<string> LoginAsync(
        HttpClient client,
        string email,
        string password = "Password123")
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                Email = email,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<LoginResult>();

        return result?.AccessToken
               ?? throw new InvalidOperationException(
                   "Login response did not contain an access token.");
    }
}