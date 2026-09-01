using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;
using WebApi.Infrastructure;

namespace WebApi.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(
        this IServiceCollection services)
    {
        services.AddControllers();

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        AddSwagger(services);
        AddRateLimiting(services);

        return services;
    }

    private static void AddSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "Conference Booking API",
                    Version = "v1",
                    Description =
                        "API для управління конференц-залами, бронюваннями та бізнес-звітами."
                });

            options.CustomSchemaIds(
                type => type.FullName?.Replace("+", ".") ?? type.Name);

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Введіть JWT access token."
                });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
            
            IncludeXmlComments(
                options,
                $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");

            IncludeXmlComments(
                options,
                "Application.xml");
        });
    }

    private static void IncludeXmlComments(
        Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options,
        string fileName)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            fileName);

        if (File.Exists(path))
        {
            options.IncludeXmlComments(path);
        }
    }

    private static void AddRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.AddPolicy(
                RateLimitingPolicies.Write,
                httpContext =>
                    CreateFixedWindowLimiter(
                        httpContext,
                        permitLimit: 30));

            options.AddPolicy(
                RateLimitingPolicies.Booking,
                httpContext =>
                    CreateFixedWindowLimiter(
                        httpContext,
                        permitLimit: 10));

            options.AddPolicy(
                RateLimitingPolicies.Reports,
                httpContext =>
                    CreateFixedWindowLimiter(
                        httpContext,
                        permitLimit: 20));

            options.OnRejected = async (
                context,
                cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        title = "Too many requests",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Request limit exceeded. Please try again later.",
                        traceId = context.HttpContext.TraceIdentifier
                    },
                    cancellationToken);
            };
        });
    }

    private static RateLimitPartition<string> CreateFixedWindowLimiter(
        HttpContext context,
        int permitLimit)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            GetClientIdentifier(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}