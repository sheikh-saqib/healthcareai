using Dapper;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Infrastructure.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class UserPasswordHistoryRepository : IRepository<UserPasswordHistory>
{
    private readonly IDbConnectionFactory _connectionFactory;
    private const string TableName = "UserPasswordHistories"; // Correct table name

    public UserPasswordHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserPasswordHistory?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"SELECT 
            ""Id"" as Id,
            ""UserPasswordHistoryId"" as UserPasswordHistoryId,
            ""UserId"" as UserId,
            ""PasswordHash"" as PasswordHash,
            ""PasswordSalt"" as PasswordSalt,
            ""HashAlgorithm"" as HashAlgorithm,
            ""ChangeReason"" as ChangeReason,
            ""ChangedByUserId"" as ChangedByUserId,
            ""IpAddress"" as IpAddress,
            ""UserAgent"" as UserAgent,
            ""CreatedAt"" as CreatedAt,
            ""CreatedBy"" as CreatedBy,
            ""UpdatedAt"" as UpdatedAt,
            ""UpdatedBy"" as UpdatedBy,
            ""IsDeleted"" as IsDeleted,
            ""DeletedAt"" as DeletedAt,
            ""DeletedBy"" as DeletedBy,
            ""RowVersion"" as RowVersion,
            ""TenantId"" as TenantId,
            ""Metadata"" as Metadata
        FROM ""UserPasswordHistories"" 
        WHERE ""Id"" = @Id";
        
        return await connection.QueryFirstOrDefaultAsync<UserPasswordHistory>(sql, new { Id = id.ToString() });
    }

    public async Task<IEnumerable<UserPasswordHistory>> GetAllAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"SELECT 
            ""Id"" as Id,
            ""UserPasswordHistoryId"" as UserPasswordHistoryId,
            ""UserId"" as UserId,
            ""PasswordHash"" as PasswordHash,
            ""PasswordSalt"" as PasswordSalt,
            ""HashAlgorithm"" as HashAlgorithm,
            ""ChangeReason"" as ChangeReason,
            ""ChangedByUserId"" as ChangedByUserId,
            ""IpAddress"" as IpAddress,
            ""UserAgent"" as UserAgent,
            ""CreatedAt"" as CreatedAt,
            ""CreatedBy"" as CreatedBy,
            ""UpdatedAt"" as UpdatedAt,
            ""UpdatedBy"" as UpdatedBy,
            ""IsDeleted"" as IsDeleted,
            ""DeletedAt"" as DeletedAt,
            ""DeletedBy"" as DeletedBy,
            ""RowVersion"" as RowVersion,
            ""TenantId"" as TenantId,
            ""Metadata"" as Metadata
        FROM ""UserPasswordHistories""";
        
        return await connection.QueryAsync<UserPasswordHistory>(sql);
    }

    public async Task AddAsync(UserPasswordHistory entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var sql = @"INSERT INTO ""UserPasswordHistories"" (
            ""Id"", ""UserPasswordHistoryId"", ""UserId"", ""PasswordHash"", ""PasswordSalt"", ""HashAlgorithm"",
            ""ChangeReason"", ""ChangedByUserId"", ""IpAddress"", ""UserAgent"",
            ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"",
            ""IsDeleted"", ""DeletedAt"", ""DeletedBy"", ""RowVersion"", ""TenantId"", ""Metadata""
        ) VALUES (
            @Id, @UserPasswordHistoryId, @UserId, @PasswordHash, @PasswordSalt, @HashAlgorithm,
            @ChangeReason, @ChangedByUserId, @IpAddress, @UserAgent,
            @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
            @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata
        )";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(UserPasswordHistory entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        
        var sql = @"UPDATE ""UserPasswordHistories"" SET
            ""UserPasswordHistoryId"" = @UserPasswordHistoryId,
            ""UserId"" = @UserId,
            ""PasswordHash"" = @PasswordHash,
            ""PasswordSalt"" = @PasswordSalt,
            ""HashAlgorithm"" = @HashAlgorithm,
            ""ChangeReason"" = @ChangeReason,
            ""ChangedByUserId"" = @ChangedByUserId,
            ""IpAddress"" = @IpAddress,
            ""UserAgent"" = @UserAgent,
            ""CreatedAt"" = @CreatedAt,
            ""CreatedBy"" = @CreatedBy,
            ""UpdatedAt"" = @UpdatedAt,
            ""UpdatedBy"" = @UpdatedBy,
            ""IsDeleted"" = @IsDeleted,
            ""DeletedAt"" = @DeletedAt,
            ""DeletedBy"" = @DeletedBy,
            ""RowVersion"" = @RowVersion,
            ""TenantId"" = @TenantId,
            ""Metadata"" = @Metadata
        WHERE ""Id"" = @Id";
        
        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(UserPasswordHistory entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"DELETE FROM ""UserPasswordHistories"" WHERE ""Id"" = @Id";
        await connection.ExecuteAsync(sql, new { Id = entity.Id });
    }

    public async Task<IEnumerable<UserPasswordHistory>> FindAsync(System.Linq.Expressions.Expression<Func<UserPasswordHistory, bool>> predicate)
    {
        // For complex queries, implement specific methods
        throw new NotImplementedException("Use specific find methods like FindByUserIdAsync");
    }

    public async Task<IEnumerable<UserPasswordHistory>> FindByUserIdAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"SELECT 
            ""Id"" as Id,
            ""UserPasswordHistoryId"" as UserPasswordHistoryId,
            ""UserId"" as UserId,
            ""PasswordHash"" as PasswordHash,
            ""PasswordSalt"" as PasswordSalt,
            ""HashAlgorithm"" as HashAlgorithm,
            ""ChangeReason"" as ChangeReason,
            ""ChangedByUserId"" as ChangedByUserId,
            ""IpAddress"" as IpAddress,
            ""UserAgent"" as UserAgent,
            ""CreatedAt"" as CreatedAt,
            ""CreatedBy"" as CreatedBy,
            ""UpdatedAt"" as UpdatedAt,
            ""UpdatedBy"" as UpdatedBy,
            ""IsDeleted"" as IsDeleted,
            ""DeletedAt"" as DeletedAt,
            ""DeletedBy"" as DeletedBy,
            ""RowVersion"" as RowVersion,
            ""TenantId"" as TenantId,
            ""Metadata"" as Metadata
        FROM ""UserPasswordHistories"" 
        WHERE ""UserId"" = @UserId
        ORDER BY ""CreatedAt"" DESC";
        
        return await connection.QueryAsync<UserPasswordHistory>(sql, new { UserId = userId });
    }
}
