using System.Data;

namespace HealthCareAI.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync();
}
