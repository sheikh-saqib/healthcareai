using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class UserSessionRepository : Repository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    // Session Management
    public async Task<UserSession?> GetByIdAsync(string sessionId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"UserSessions\" WHERE \"Id\" = @SessionId";
        return await connection.QueryFirstOrDefaultAsync<UserSession>(sql, new { SessionId = sessionId });
    }

    public async Task<UserSession?> GetBySessionTokenAsync(string sessionToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"UserSessions\" WHERE \"SessionToken\" = @SessionToken AND \"IsActive\" = true";
        return await connection.QueryFirstOrDefaultAsync<UserSession>(sql, new { SessionToken = sessionToken });
    }

    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"UserSessions\" WHERE \"RefreshToken\" = @RefreshToken AND \"IsActive\" = true";
        return await connection.QueryFirstOrDefaultAsync<UserSession>(sql, new { RefreshToken = refreshToken });
    }

    public async Task<IEnumerable<UserSession>> GetByUserIdAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"UserSessions\" WHERE \"UserId\" = @UserId AND \"IsActive\" = true";
        return await connection.QueryAsync<UserSession>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<UserSession>> GetExpiredSessionsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"UserSessions\" WHERE \"ExpiresAt\" < @CurrentTime AND \"IsActive\" = true";
        return await connection.QueryAsync<UserSession>(sql, new { CurrentTime = DateTime.UtcNow });
    }

    // Session Operations
    public new async Task<UserSession> AddAsync(UserSession session)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        // Set default values
        session.CreatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        session.IsActive = true;

        var sql = @"SELECT * FROM sp_upsertsession(
            @Id, @UserSessionId, @UserId, @SessionToken, @RefreshToken, @JwtTokenId,
            @ExpiresAt, @LastAccessedAt, @IsActive, @DeviceId, @DeviceName, @DeviceType,
            @IpAddress, @UserAgent, @Location, @DeviceFingerprint, @IsTrustedDevice,
            @RequireTwoFactor, @LoginMethod, @OrganizationId, @SelectedRoleId, @SessionData,
            @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
            @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata
        )";

        var parameters = new
        {
            Id = session.Id,
            UserSessionId = session.UserSessionId,
            UserId = session.UserId,
            SessionToken = session.SessionToken,
            RefreshToken = session.RefreshToken,
            JwtTokenId = session.JwtTokenId,
            ExpiresAt = session.ExpiresAt,
            LastAccessedAt = session.LastAccessedAt,
            IsActive = session.IsActive,
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName,
            DeviceType = session.DeviceType,
            IpAddress = session.IpAddress,
            UserAgent = session.UserAgent,
            Location = session.Location,
            DeviceFingerprint = session.DeviceFingerprint,
            IsTrustedDevice = session.IsTrustedDevice,
            RequireTwoFactor = session.RequireTwoFactor,
            LoginMethod = session.LoginMethod,
            OrganizationId = session.OrganizationId,
            SelectedRoleId = session.SelectedRoleId,
            SessionData = session.SessionData,
            CreatedAt = session.CreatedAt,
            CreatedBy = session.CreatedBy,
            UpdatedAt = session.UpdatedAt,
            UpdatedBy = session.UpdatedBy,
            IsDeleted = session.IsDeleted,
            DeletedAt = session.DeletedAt,
            DeletedBy = session.DeletedBy,
            RowVersion = session.RowVersion,
            TenantId = session.TenantId,
            Metadata = session.Metadata
        };

        await connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);
        return session;
    }

    public async Task<UserSession> CreateAsync(UserSession session)
    {
        return await AddAsync(session);
    }

    public new async Task UpdateAsync(UserSession session)
    {
        await base.UpdateAsync(session);
    }

    public new async Task DeleteAsync(UserSession session)
    {
        await base.DeleteAsync(session);
    }

    public async Task DeactivateSessionAsync(string sessionToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"UserSessions\" SET \"IsActive\" = false WHERE \"SessionToken\" = @SessionToken";
        await connection.ExecuteAsync(sql, new { SessionToken = sessionToken });
    }

    public async Task DeactivateAllUserSessionsAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"UserSessions\" SET \"IsActive\" = false WHERE \"UserId\" = @UserId";
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }

    public async Task DeactivateExpiredSessionsAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"UserSessions\" SET \"IsActive\" = false WHERE \"ExpiresAt\" < @CurrentTime AND \"IsActive\" = true";
        await connection.ExecuteAsync(sql, new { CurrentTime = DateTime.UtcNow });
    }

    public async Task<bool> DeactivateAsync(string sessionId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"UserSessions\" SET \"IsActive\" = false WHERE \"Id\" = @SessionId";
        var rowsAffected = await connection.ExecuteAsync(sql, new { SessionId = sessionId });
        return rowsAffected > 0;
    }

    public async Task<bool> DeactivateAllByUserIdAsync(string userId, string? excludeSessionId = null)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        string sql;
        object parameters;
        
        if (excludeSessionId != null)
        {
            sql = "UPDATE \"UserSessions\" SET \"IsActive\" = false WHERE \"UserId\" = @UserId AND \"Id\" != @ExcludeSessionId";
            parameters = new { UserId = userId, ExcludeSessionId = excludeSessionId };
        }
        else
        {
            sql = "UPDATE \"UserSessions\" SET \"IsActive\" = false WHERE \"UserId\" = @UserId";
            parameters = new { UserId = userId };
        }
        
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    // Utility methods
    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"UserSessions\"";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
}
