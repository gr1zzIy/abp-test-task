using Application.ConferenceRooms.Create;
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
        
        return services;
    }
}