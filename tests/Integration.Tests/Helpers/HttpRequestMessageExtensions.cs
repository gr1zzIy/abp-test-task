using System.Net.Http.Headers;

namespace Integration.Tests.Helpers;

public static class HttpRequestMessageExtensions
{
    public static HttpRequestMessage WithBearerToken(
        this HttpRequestMessage request,
        string accessToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        return request;
    }
}