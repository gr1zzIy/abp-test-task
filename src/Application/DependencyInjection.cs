using Application.ConferenceRooms.Create;
using Application.ConferenceRooms.Delete;
using Application.ConferenceRooms.GetAll;
using Application.ConferenceRooms.GetById;
using Application.ConferenceRooms.SearchAvailable;
using Application.ConferenceRooms.Update;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<CreateConferenceRoomHandler>();
        services.AddScoped<UpdateConferenceRoomHandler>();
        services.AddScoped<GetConferenceRoomsHandler>();
        services.AddScoped<GetConferenceRoomByIdHandler>();
        services.AddScoped<DeleteConferenceRoomHandler>();
        services.AddScoped<SearchAvailableConferenceRoomsHandler>();
        
        return services;
    }
}