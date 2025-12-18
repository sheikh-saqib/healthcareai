using Dapper;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using System.Data;

namespace HealthCareAI.Infrastructure.Repositories;

public class AccessPermissionRepository : Repository<AccessPermission>, IAccessPermissionRepository
{
    public AccessPermissionRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<AccessPermission?> GetByIdAsync(string permissionId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"AccessPermissions\" WHERE \"Id\" = @PermissionId";
        return await connection.QueryFirstOrDefaultAsync<AccessPermission>(sql, new { PermissionId = permissionId });
    }

    public async Task<IEnumerable<AccessPermission>> GetByRoleIdAsync(string roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"AccessPermissions\" WHERE \"RoleId\" = @RoleId";
        return await connection.QueryAsync<AccessPermission>(sql, new { RoleId = roleId });
    }

    public async Task<IEnumerable<AccessPermission>> GetByRoleIdsAsync(List<string> roleIds)
    {
        if (!roleIds.Any()) return Enumerable.Empty<AccessPermission>();
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"AccessPermissions\" WHERE \"RoleId\" = ANY(@RoleIds)";
        return await connection.QueryAsync<AccessPermission>(sql, new { RoleIds = roleIds.ToArray() });
    }

    public async Task<IEnumerable<AccessPermission>> GetByResourceAsync(string resource)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"AccessPermissions\" WHERE \"Resource\" = @Resource";
        return await connection.QueryAsync<AccessPermission>(sql, new { Resource = resource });
    }

    public async Task<AccessPermission?> GetByRoleAndResourceAsync(string roleId, string resource)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM \"AccessPermissions\" WHERE \"RoleId\" = @RoleId AND \"Resource\" = @Resource";
        return await connection.QueryFirstOrDefaultAsync<AccessPermission>(sql, new { RoleId = roleId, Resource = resource });
    }

    public async Task<bool> HasPermissionAsync(string roleId, string resource, string action)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT COUNT(*)
            FROM ""AccessPermissions"" ap
            JOIN ""AccessRoles"" ar ON ap.""RoleId"" = ar.""Id""
            WHERE ap.""RoleId"" = @RoleId 
                  AND ap.""Resource"" = @Resource 
                  AND ap.""Action"" = @Action 
                  AND ap.""IsGranted"" = true
                  AND ar.""IsActive"" = true";
        
        var count = await connection.ExecuteScalarAsync<int>(sql, new { RoleId = roleId, Resource = resource, Action = action });
        return count > 0;
    }

    public async Task<IEnumerable<string>> GetResourcesForRoleAsync(string roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT DISTINCT \"Resource\" FROM \"AccessPermissions\" WHERE \"RoleId\" = @RoleId AND \"IsGranted\" = true";
        return await connection.QueryAsync<string>(sql, new { RoleId = roleId });
    }

    // Permission Evaluation
    public async Task<IEnumerable<string>> GetAllowedActionsAsync(List<string> roleIds, string resource)
    {
        if (!roleIds.Any()) return Enumerable.Empty<string>();
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT DISTINCT ap.""Action""
            FROM ""AccessPermissions"" ap
            JOIN ""AccessRoles"" ar ON ap.""RoleId"" = ar.""Id""
            WHERE ap.""RoleId"" = ANY(@RoleIds) 
                  AND ap.""Resource"" = @Resource 
                  AND ap.""IsGranted"" = true
                  AND ar.""IsActive"" = true";
        
        return await connection.QueryAsync<string>(sql, new { RoleIds = roleIds.ToArray(), Resource = resource });
    }

    public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT DISTINCT ap.""Resource"" || ':' || ap.""Action"" as Permission
            FROM ""AccessPermissions"" ap
            JOIN ""AccessRoles"" ar ON ap.""RoleId"" = ar.""Id""
            WHERE ap.""RoleId"" = @RoleId 
                  AND ap.""IsGranted"" = true
                  AND ar.""IsActive"" = true";
        
        return await connection.QueryAsync<string>(sql, new { RoleId = roleId });
    }

    public async Task<IEnumerable<string>> GetPermissionsForRolesAsync(List<string> roleIds)
    {
        if (!roleIds.Any()) return Enumerable.Empty<string>();
        
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT DISTINCT ap.""Resource"" || ':' || ap.""Action"" as Permission
            FROM ""AccessPermissions"" ap
            JOIN ""AccessRoles"" ar ON ap.""RoleId"" = ar.""Id""
            WHERE ap.""RoleId"" = ANY(@RoleIds) 
                  AND ap.""IsGranted"" = true
                  AND ar.""IsActive"" = true";
        
        return await connection.QueryAsync<string>(sql, new { RoleIds = roleIds.ToArray() });
    }

    // UserService Required Methods
    public async Task<IEnumerable<string>> GetUserPermissionsAsync(string userId)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT DISTINCT ap.""Resource"" || ':' || ap.""Action"" as Permission
            FROM ""AccessPermissions"" ap
            JOIN ""AccessRoles"" ar ON ap.""AccessRoleId"" = ar.""AccessRoleId""
            JOIN ""UserRoles"" ur ON ar.""AccessRoleId"" = ur.""AccessRoleId""
            WHERE ur.""UserId"" = @UserId 
                  AND ap.""IsAllowed"" = true
                  AND ar.""IsActive"" = true
                  AND ur.""IsActive"" = true";
        
        return await connection.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<string>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId)
    {
        // For now, return the same as GetUserPermissionsAsync since we don't have organization-specific permissions
        return await GetUserPermissionsAsync(userId);
    }

    public new async Task<AccessPermission> AddAsync(AccessPermission accessPermission)
    {
        accessPermission.CreatedAt = DateTime.UtcNow;
        await base.AddAsync(accessPermission);
        return accessPermission;
    }

    public new async Task UpdateAsync(AccessPermission accessPermission)
    {
        await base.UpdateAsync(accessPermission);
    }

    public new async Task DeleteAsync(AccessPermission accessPermission)
    {
        await base.DeleteAsync(accessPermission);
    }

    public async Task<int> GetCountAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT COUNT(*) FROM \"AccessPermissions\"";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task SaveChangesAsync()
    {
        // For Dapper, changes are saved immediately, so this method doesn't need to do anything
        await Task.CompletedTask;
    }
} 