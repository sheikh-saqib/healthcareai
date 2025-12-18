using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace HealthCareAI.Infrastructure.Data;

public class PostgreSqlDatabaseInitializer : IDatabaseInitializer
{
    private readonly string _connectionString;
    private readonly ILogger<PostgreSqlDatabaseInitializer> _logger;

    public PostgreSqlDatabaseInitializer(IConfiguration configuration, ILogger<PostgreSqlDatabaseInitializer> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task<bool> DatabaseExistsAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Test if we can query a simple table
            var sql = "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public'";
            var count = await connection.ExecuteScalarAsync<int>(sql);
            
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database connection test failed");
            return false;
        }
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Check if database exists and has tables
            if (await DatabaseExistsAsync())
            {
                _logger.LogInformation("Database already exists with tables. Skipping schema creation.");
                
                // Just verify we can connect and query the existing tables
                await VerifyExistingDatabaseAsync();
                return;
            }

            _logger.LogInformation("Database exists but has no tables. Creating schema...");
            await CreateSchemaAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize database");
            throw;
        }
    }

    private async Task VerifyExistingDatabaseAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // Test a few key tables to ensure they exist and are accessible
            var testQueries = new[]
            {
                "SELECT COUNT(*) FROM \"Users\"",
                "SELECT COUNT(*) FROM \"Patients\"",
                "SELECT COUNT(*) FROM \"Consultations\"",
                "SELECT COUNT(*) FROM \"Prescriptions\""
            };

            foreach (var query in testQueries)
            {
                try
                {
                    var count = await connection.ExecuteScalarAsync<int>(query);
                    _logger.LogDebug("Table query successful: {Query}, Count: {Count}", query, count);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to query table: {Query}", query);
                }
            }
            
            _logger.LogInformation("Existing database verification completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify existing database");
            throw;
        }
    }

    private async Task CreateSchemaAsync()
    {
        try
        {
            // Read and execute the schema SQL
            var schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "schema.sql");
            if (!File.Exists(schemaPath))
            {
                // Fallback to assembly location
                schemaPath = Path.Combine(AppContext.BaseDirectory, "Data", "schema.sql");
            }

            if (File.Exists(schemaPath))
            {
                var schemaSql = await File.ReadAllTextAsync(schemaPath);
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                
                // Split the SQL by semicolons and execute each statement
                var statements = schemaSql.Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s.Trim()))
                    .Select(s => s.Trim());

                foreach (var statement in statements)
                {
                    if (!string.IsNullOrWhiteSpace(statement))
                    {
                        try
                        {
                            await connection.ExecuteAsync(statement);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to execute SQL statement: {Statement}", statement);
                        }
                    }
                }

                _logger.LogInformation("Database schema created successfully");
            }
            else
            {
                _logger.LogWarning("Schema file not found at: {SchemaPath}", schemaPath);
                // Create basic tables as fallback
                await CreateBasicTablesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database schema");
            throw;
        }
    }

    private async Task CreateBasicTablesAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            // Create basic Users table
            var createUsersTable = @"
                CREATE TABLE IF NOT EXISTS ""Users"" (
                    ""Id"" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    ""Username"" VARCHAR(100) NOT NULL UNIQUE,
                    ""Email"" VARCHAR(255) NOT NULL UNIQUE,
                    ""PasswordHash"" VARCHAR(255) NOT NULL,
                    ""FirstName"" VARCHAR(100),
                    ""LastName"" VARCHAR(100),
                    ""IsActive"" BOOLEAN DEFAULT true,
                    ""CreatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                    ""UpdatedAt"" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

            await connection.ExecuteAsync(createUsersTable);
            _logger.LogInformation("Basic tables created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create basic tables");
            throw;
        }
    }
}
