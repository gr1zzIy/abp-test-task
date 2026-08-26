using Application;
using Infrastructure;
using WebApi.Infrastructure;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        RateLimitingPolicies.Write,
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetClientIdentifier(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.AddPolicy(
        RateLimitingPolicies.Booking,
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetClientIdentifier(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    
    options.AddPolicy(
        RateLimitingPolicies.Reports,
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                GetClientIdentifier(httpContext),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    options.OnRejected = async (context, cancellationToken) =>
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

static string GetClientIdentifier(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString()
           ?? "unknown";
}

var app = builder.Build();

app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseRateLimiter();

app.MapControllers();

app.Run();