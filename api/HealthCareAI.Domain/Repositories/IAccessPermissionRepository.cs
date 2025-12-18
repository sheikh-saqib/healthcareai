using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IAccessPermissionRepository
{
    // Basic CRUD Operations
    Task<AccessPermission?> GetByIdAsync(string permissionId);
    Task<AccessPermission> AddAsync(AccessPermission permission);
    Task UpdateAsync(AccessPermission permission);
    Task DeleteAsync(AccessPermission permission);

    // Core Permission Queries
    Task<IEnumerable<AccessPermission>> GetByRoleIdAsync(string roleId);
    Task<IEnumerable<AccessPermission>> GetByRoleIdsAsync(List<string> roleIds);
    Task<IEnumerable<AccessPermission>> GetByResourceAsync(string resource);
    Task<AccessPermission?> GetByRoleAndResourceAsync(string roleId, string resource);
    Task<bool> HasPermissionAsync(string roleId, string resource, string action);
    Task<IEnumerable<string>> GetResourcesForRoleAsync(string roleId);

    // Permission Evaluation
    Task<IEnumerable<string>> GetAllowedActionsAsync(List<string> roleIds, string resource);
    Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleId);
    Task<IEnumerable<string>> GetPermissionsForRolesAsync(List<string> roleIds);

    // UserService Required Methods
    Task<IEnumerable<string>> GetUserPermissionsAsync(string userId);
    Task<IEnumerable<string>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId);

    // Utility methods
    Task<int> GetCountAsync();
    Task SaveChangesAsync();
} 