using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using HealthCareAI.Infrastructure.Repositories;
using HealthCareAI.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HealthCareAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Connection Factory
        services.AddSingleton<IDbConnectionFactory, PostgreSqlConnectionFactory>();
        
        // Database Initializer
        services.AddScoped<IDatabaseInitializer, PostgreSqlDatabaseInitializer>();
        
        // Generic Repository
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Specific Repository overrides
        services.AddScoped<IRepository<Domain.Entities.User>, UserRepository>();
        services.AddScoped<IRepository<Domain.Entities.UserPasswordHistory>, UserPasswordHistoryRepository>();
        
        // Services
        services.AddHttpClient<IOpenAIService, OpenAIService>()
            .AddStandardResilienceHandler();
        services.AddScoped<IConsultationService, ConsultationService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IStatsService, StatsService>();
        
        // Repositories
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        
        // Authentication Repositories
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IAccessPermissionRepository, AccessPermissionRepository>();

        // Authentication Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IUserService, UserService>();
        

        return services;
    }
} 