using Application;
using Infrastructure;
using WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddWebApi();

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapHealthCheckEndpoints();

app.Run();

public partial class Program;