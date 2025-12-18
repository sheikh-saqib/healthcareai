using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class VerificationTokenRepository : Repository<VerificationToken>, IVerificationTokenRepository
{


    public VerificationTokenRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<VerificationToken?> GetByIdAsync(string tokenId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"Id\" = @TokenId";
        return await connection.QueryFirstOrDefaultAsync<VerificationToken>(sql, new { TokenId = tokenId });
    }

    public async Task<VerificationToken?> GetByTokenAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"Token\" = @Token";
        return await connection.QueryFirstOrDefaultAsync<VerificationToken>(sql, new { Token = token });
    }

    public async Task<VerificationToken?> GetByTokenAndTypeAsync(string token, string tokenType)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"Token\" = @Token AND \"TokenType\" = @TokenType";
        return await connection.QueryFirstOrDefaultAsync<VerificationToken>(sql, new { Token = token, TokenType = tokenType });
    }

    public async Task<IEnumerable<VerificationToken>> GetByUserIdAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"UserId\" = @UserId ORDER BY \"CreatedAt\" DESC";
        return await connection.QueryAsync<VerificationToken>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<VerificationToken>> GetByUserIdAndTypeAsync(string userId, string tokenType)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"UserId\" = @UserId AND \"TokenType\" = @TokenType ORDER BY \"CreatedAt\" DESC";
        return await connection.QueryAsync<VerificationToken>(sql, new { UserId = userId, TokenType = tokenType });
    }

    public async Task<IEnumerable<VerificationToken>> GetByTypeAsync(string type)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"TokenType\" = @Type ORDER BY \"CreatedAt\" DESC";
        return await connection.QueryAsync<VerificationToken>(sql, new { Type = type });
    }

    public async Task<IEnumerable<VerificationToken>> GetExpiredTokensAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"ExpiresAt\" < @CurrentTime";
        return await connection.QueryAsync<VerificationToken>(sql, new { CurrentTime = DateTime.UtcNow });
    }

    // Token Operations
    public async Task<bool> MarkAsUsedAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"VerificationTokens\" SET \"UsedAt\" = @UsedAt WHERE \"Token\" = @Token";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Token = token, UsedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> InvalidateTokensByUserIdAsync(string userId, string tokenType)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        var sql = "UPDATE \"VerificationTokens\" SET \"UsedAt\" = @UsedAt WHERE \"UserId\" = @UserId AND \"TokenType\" = @TokenType AND \"UsedAt\" IS NULL";
        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, TokenType = tokenType, UsedAt = DateTime.UtcNow });

        return rowsAffected > 0;
    }

    // Token Validation
    public async Task<bool> IsTokenValidAsync(string token, string tokenType)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"VerificationTokens\" WHERE \"Token\" = @Token AND \"TokenType\" = @TokenType AND \"UsedAt\" IS NULL AND \"ExpiresAt\" > @CurrentTime";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Token = token, TokenType = tokenType, CurrentTime = DateTime.UtcNow });
        return count > 0;
    }

    public async Task<bool> IsTokenExpiredAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"VerificationTokens\" WHERE \"Token\" = @Token AND \"ExpiresAt\" <= @CurrentTime";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Token = token, CurrentTime = DateTime.UtcNow });
        return count > 0;
    }

    public async Task<bool> IsTokenUsedAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"VerificationTokens\" WHERE \"Token\" = @Token AND \"UsedAt\" IS NOT NULL";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Token = token });
        return count > 0;
    }

    public async Task<bool> CanUseTokenAsync(string token, string tokenType)
    {
        return await IsTokenValidAsync(token, tokenType);
    }

    // Token Queries
    public async Task<VerificationToken?> GetLatestTokenAsync(string userId, string tokenType)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"VerificationTokens\" WHERE \"UserId\" = @UserId AND \"TokenType\" = @TokenType ORDER BY \"CreatedAt\" DESC LIMIT 1";
        return await connection.QueryFirstOrDefaultAsync<VerificationToken>(sql, new { UserId = userId, TokenType = tokenType });
    }

    public async Task<int> GetAttemptCountAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT \"AttemptCount\" FROM \"VerificationTokens\" WHERE \"Token\" = @Token";
        return await connection.ExecuteScalarAsync<int>(sql, new { Token = token });
    }

    public async Task<bool> IncrementAttemptCountAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"VerificationTokens\" SET \"AttemptCount\" = \"AttemptCount\" + 1, \"LastAttemptAt\" = @CurrentTime WHERE \"Token\" = @Token";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Token = token, CurrentTime = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    public async Task<bool> HasExceededMaxAttemptsAsync(string token)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"VerificationTokens\" WHERE \"Token\" = @Token AND \"AttemptCount\" >= \"MaxAttempts\"";
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Token = token });
        return count > 0;
    }

    // Token Cleanup
    public async Task<int> CleanupExpiredTokensAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "DELETE FROM \"VerificationTokens\" WHERE \"ExpiresAt\" < @CurrentTime";
        return await connection.ExecuteAsync(sql, new { CurrentTime = DateTime.UtcNow });
    }

    public async Task<int> CleanupUsedTokensAsync(int daysToKeep)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        var sql = "DELETE FROM \"VerificationTokens\" WHERE \"UsedAt\" IS NOT NULL AND \"UsedAt\" < @CutoffDate";
        return await connection.ExecuteAsync(sql, new { CutoffDate = cutoffDate });
    }

    public new async Task<VerificationToken> AddAsync(VerificationToken verificationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        verificationToken.CreatedAt = DateTime.UtcNow;
        verificationToken.UpdatedAt = DateTime.UtcNow;

        var sql = @"
INSERT INTO ""VerificationTokens"" (
    ""VerificationTokenId"", ""UserId"", ""Token"", ""TokenType"", ""Email"", ""Phone"",
    ""ExpiresAt"", ""IsUsed"", ""UsedAt"", ""IpAddress"", ""UserAgent"",
    ""AttemptCount"", ""LastAttemptAt"", ""MaxAttempts"", ""Purpose"",
    ""Id"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"",
    ""IsDeleted"", ""DeletedAt"", ""DeletedBy"", ""RowVersion"", ""TenantId"", ""Metadata""
) VALUES (
    @VerificationTokenId, @UserId, @Token, @TokenType, @Email, @Phone,
    @ExpiresAt, @IsUsed, @UsedAt, @IpAddress, @UserAgent,
    @AttemptCount, @LastAttemptAt, @MaxAttempts, @Purpose,
    @Id, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
    @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata
)";


        await connection.ExecuteAsync(sql, verificationToken);
        return verificationToken;
    }

    public async Task<VerificationToken> CreateAsync(VerificationToken verificationToken)
    {
        return await AddVerificationTokenAsync(verificationToken);
    }

    public async Task<VerificationToken> AddVerificationTokenAsync(VerificationToken verificationToken)
    {
        return await AddAsync(verificationToken);
    }

    public new async Task UpdateAsync(VerificationToken verificationToken)
    {
        await base.UpdateAsync(verificationToken);
    }

    public new async Task DeleteAsync(VerificationToken verificationToken)
    {
        await base.DeleteAsync(verificationToken);
    }

    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"VerificationTokens\"";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
}