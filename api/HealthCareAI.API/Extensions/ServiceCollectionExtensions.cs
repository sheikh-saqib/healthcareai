using HealthCareAI.Application.Behaviors;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Infrastructure.Data;
using HealthCareAI.Infrastructure.Repositories;
using MediatR;

using FluentValidation;

namespace HealthCareAI.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register Application Layer Services
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));
        
        // Add Validation Pipeline Behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        
        // Register Validators
        services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);
        
        return services;
    }


} 