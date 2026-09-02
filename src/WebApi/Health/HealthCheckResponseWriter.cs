using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebApi.Health;

/// <summary>
/// Формує структуровану відповідь health check endpoint-ів.
/// </summary>
public static class HealthCheckResponseWriter
{
    public static async Task WriteAsync(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}