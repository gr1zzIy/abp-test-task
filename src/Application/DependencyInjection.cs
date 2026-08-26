using Application.Abstractions.Pricing;
using Application.Bookings.Create;
using Application.ConferenceRooms.Create;
using Application.ConferenceRooms.Delete;
using Application.ConferenceRooms.GetAll;
using Application.ConferenceRooms.GetById;
using Application.ConferenceRooms.SearchAvailable;
using Application.ConferenceRooms.Update;
using Application.Pricing;
using Application.Reports.PopularServices;
using Application.Reports.Revenue;
using Application.Reports.RoomUtilization;
using Application.Services.GetAll;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddSingleton<IRentalPriceCalculator, RentalPriceCalculator>();
        
        services.AddScoped<CreateConferenceRoomHandler>();
        services.AddScoped<UpdateConferenceRoomHandler>();
        services.AddScoped<GetConferenceRoomsHandler>();
        services.AddScoped<GetConferenceRoomByIdHandler>();
        services.AddScoped<DeleteConferenceRoomHandler>();
        services.AddScoped<SearchAvailableConferenceRoomsHandler>();
        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<GetServicesHandler>();
        services.AddScoped<RevenueReportHandler>();
        services.AddScoped<RoomUtilizationHandler>();
        services.AddScoped<PopularServicesHandler>();
        services.AddScoped<RevenueReportHandler>();
        
        return services;
    }
}