namespace HealthCareAI.Infrastructure.Data;

public interface IDatabaseInitializer
{
    Task InitializeAsync();
    Task<bool> DatabaseExistsAsync();
}
