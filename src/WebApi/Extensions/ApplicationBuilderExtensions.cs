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
}