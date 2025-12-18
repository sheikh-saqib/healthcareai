using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public new async Task<IEnumerable<Patient>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            ORDER BY ""FirstName"", ""LastName""";
        
        return await connection.QueryAsync<Patient>(sql);
    }

    public async Task<Patient?> GetByNameAsync(string name)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            WHERE ""FirstName"" ILIKE @Name OR ""LastName"" ILIKE @Name OR ""Name"" ILIKE @Name";
        
        var searchTerm = $"%{name}%";
        return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { Name = searchTerm });
    }

    public async Task<Patient?> GetByIdAsync(string id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            WHERE ""PatientId"" = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Patient>> GetActiveAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            WHERE ""IsActive"" = true";
        
        return await connection.QueryAsync<Patient>(sql);
    }

    public async Task<IEnumerable<Patient>> GetInactiveAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            WHERE ""IsActive"" = false";
        
        return await connection.QueryAsync<Patient>(sql);
    }

    public async Task<Patient?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            WHERE ""Email"" = @Email";
        
        return await connection.QueryFirstOrDefaultAsync<Patient>(sql, new { Email = email });
    }

    public new async Task<Patient> AddAsync(Patient patient)
    {
        patient.CreatedAt = DateTime.UtcNow;
        await base.AddAsync(patient);
        return patient;
    }

    public new async Task UpdateAsync(Patient patient)
    {
        patient.UpdatedAt = DateTime.UtcNow;
        await base.UpdateAsync(patient);
    }

    public new async Task DeleteAsync(Patient patient)
    {
        await base.DeleteAsync(patient);
    }

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM vw_patients
            WHERE ""FirstName"" ILIKE @SearchTerm OR 
                  ""LastName"" ILIKE @SearchTerm OR 
                  ""Name"" ILIKE @SearchTerm OR
                  ""Email"" ILIKE @SearchTerm OR
                  ""Phone"" ILIKE @SearchTerm OR
                  ""PatientId"" ILIKE @SearchTerm";
        
        var searchPattern = $"%{searchTerm}%";
        return await connection.QueryAsync<Patient>(sql, new { SearchTerm = searchPattern });
    }

    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM vw_patients";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
} 