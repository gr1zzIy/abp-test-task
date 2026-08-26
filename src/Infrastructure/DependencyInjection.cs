using Application.Abstractions.Persistence;
using Application.Abstractions.Reporting;
using Application.Abstractions.Time;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var businessTimeZoneId = configuration["Booking:TimeZone"]
                                 ?? throw new InvalidOperationException(
                                     "Booking time zone is not configured.");

        services.AddSingleton<IBusinessTimeZone>(
            new BusinessTimeZone(businessTimeZoneId));
        
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' was not found.");
        
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IConferenceRoomRepository, ConferenceRoomRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        
        
        
        return services;
    }
}