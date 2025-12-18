using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<UserRole?> GetByIdAsync(string userRoleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT ur.*, ar.""Name"" as RoleName, ar.""Description"" as RoleDescription
            FROM ""UserRoles"" ur
            LEFT JOIN ""AccessRoles"" ar ON ur.""AccessRoleId"" = ar.""Id""
            WHERE ur.""Id"" = @UserRoleId";
        
        return await connection.QueryFirstOrDefaultAsync<UserRole>(sql, new { UserRoleId = userRoleId });
    }

    public async Task<IEnumerable<UserRole>> GetByUserIdAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT ur.*, ar.""RoleName"" as RoleName, ar.""Description"" as RoleDescription
            FROM ""UserRoles"" ur
            LEFT JOIN ""AccessRoles"" ar ON ur.""AccessRoleId"" = ar.""Id""
            WHERE ur.""UserId"" = @UserId AND ur.""IsActive"" = true";
        
        return await connection.QueryAsync<UserRole>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(string roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT ur.*, ar.""Name"" as RoleName, ar.""Description"" as RoleDescription
            FROM ""UserRoles"" ur
            LEFT JOIN ""AccessRoles"" ar ON ur.""AccessRoleId"" = ar.""Id""
            WHERE ur.""AccessRoleId"" = @RoleId AND ur.""IsActive"" = true";
        
        return await connection.QueryAsync<UserRole>(sql, new { RoleId = roleId });
    }

    public async Task<UserRole?> GetByUserIdAndRoleIdAsync(string userId, string roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT ur.*, ar.""Name"" as RoleName, ar.""Description"" as RoleDescription
            FROM ""UserRoles"" ur
            LEFT JOIN ""AccessRoles"" ar ON ur.""AccessRoleId"" = ar.""Id""
            WHERE ur.""UserId"" = @UserId AND ur.""AccessRoleId"" = @RoleId AND ur.""IsActive"" = true";
        
        return await connection.QueryFirstOrDefaultAsync<UserRole>(sql, new { UserId = userId, RoleId = roleId });
    }

    public async Task<bool> HasRoleAsync(string userId, string roleName)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT COUNT(*)
            FROM ""UserRoles"" ur
            JOIN ""AccessRoles"" ar ON ur.""AccessRoleId"" = ar.""Id""
            WHERE ur.""UserId"" = @UserId AND ar.""Name"" = @RoleName AND ur.""IsActive"" = true";
        
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, RoleName = roleName });
        return count > 0;
    }

    public async Task<IEnumerable<string>> GetUserRoleNamesAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT ar.""Name""
            FROM ""UserRoles"" ur
            JOIN ""AccessRoles"" ar ON ur.""AccessRoleId"" = ar.""Id""
            WHERE ur.""UserId"" = @UserId AND ur.""IsActive"" = true";
        
        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    // Role Management
    public async Task DeactivateUserRoleAsync(string userId, string roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE \"UserRoles\" SET \"IsActive\" = false WHERE \"UserId\" = @UserId AND \"AccessRoleId\" = @RoleId";
        await connection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }

    // UserService Required Methods
    public async Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId)
    {
        return await GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<UserRole>> GetUserRolesByOrganizationAsync(string userId, string organizationId)
    {
        // For now, return the same as GetByUserIdAsync since we don't have organization-specific roles
        return await GetByUserIdAsync(userId);
    }

    public async Task<IEnumerable<UserRole>> GetUsersByRoleAsync(string roleId)
    {
        return await GetByRoleIdAsync(roleId);
    }

    public new async Task<UserRole> AddAsync(UserRole userRole)
    {
        userRole.AssignedAt = DateTime.UtcNow;
        userRole.IsActive = true;
        await base.AddAsync(userRole);
        return userRole;
    }

    public new async Task UpdateAsync(UserRole userRole)
    {
        await base.UpdateAsync(userRole);
    }

    public new async Task DeleteAsync(UserRole userRole)
    {
        await base.DeleteAsync(userRole);
    }

    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"UserRoles\"";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
}
