using Application.Abstractions.Pricing;
using Application.Pricing;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Валідатори FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Системний час та калькулятор ціни
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IRentalPriceCalculator, RentalPriceCalculator>();

        // Автоматична реєстрація всіх Handlers (закінчуються на "Handler")
        var handlerTypes = assembly.GetTypes()
            .Where(type => 
                type.IsClass && 
                !type.IsAbstract && 
                type.Name.EndsWith("Handler"));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }

        return services;
    }
}