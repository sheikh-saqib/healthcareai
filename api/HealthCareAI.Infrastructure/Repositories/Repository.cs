using Dapper;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Infrastructure.Data;
using System.Data;
using System.Linq.Expressions;

namespace HealthCareAI.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly IDbConnectionFactory _connectionFactory;
    protected readonly string _tableName;

    public Repository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
        _tableName = typeof(T).Name + "s"; // Simple pluralization
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM \"{_tableName}\" WHERE \"Id\" = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM \"{_tableName}\"";
        return await connection.QueryAsync<T>(sql);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        // Note: Dapper doesn't support LINQ expressions directly
        // This is a simplified implementation - in practice, you'd need to build SQL dynamically
        // or use a different approach for complex queries
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM \"{_tableName}\"";
        var results = await connection.QueryAsync<T>(sql);
        
        // Apply the predicate in memory (not ideal for large datasets)
        return results.AsQueryable().Where(predicate);
    }

    public async Task AddAsync(T entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.CanRead)
            .ToList();
        
        var columns = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
        var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));
        
        var sql = $"INSERT INTO \"{_tableName}\" ({columns}) VALUES ({parameters})";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(T entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && p.CanRead)
            .ToList();
        
        var setClause = string.Join(", ", properties.Select(p => $"\"{p.Name}\" = @{p.Name}"));
        var sql = $"UPDATE \"{_tableName}\" SET {setClause} WHERE \"Id\" = @Id";
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(T entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var id = typeof(T).GetProperty("Id")?.GetValue(entity);
        if (id != null)
        {
            var sql = $"DELETE FROM \"{_tableName}\" WHERE \"Id\" = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }
    }
} 