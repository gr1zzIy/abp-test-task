using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using WebApi.Health;

namespace WebApi.Extensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseSwaggerDocumentation(
        this WebApplication app)
    {
        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "Conference Booking API v1");

            options.DocumentTitle =
                "Conference Booking API";
        });

        return app;
    }
    
    public static WebApplication MapHealthCheckEndpoints(
        this WebApplication app)
    {
        app.MapHealthChecks(
                "/health/live",
                new HealthCheckOptions
                {
                    // Liveness перевіряє лише сам процес застосунку.
                    Predicate = _ => false,
                    ResponseWriter = HealthCheckResponseWriter.WriteAsync
                })
            .AllowAnonymous();

        app.MapHealthChecks(
                "/health/ready",
                new HealthCheckOptions
                {
                    // Readiness включає залежності, без яких API
                    // не може коректно обробляти запити.
                    Predicate = check =>
                        check.Tags.Contains("ready"),
                    ResponseWriter = HealthCheckResponseWriter.WriteAsync
                })
            .AllowAnonymous();

        return app;
    }
}