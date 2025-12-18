using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IUserRoleRepository
{
    // Basic CRUD Operations
    Task<UserRole?> GetByIdAsync(string userRoleId);
    Task<UserRole> AddAsync(UserRole userRole);
    Task UpdateAsync(UserRole userRole);
    Task DeleteAsync(UserRole userRole);

    // Core Role Queries
    Task<IEnumerable<UserRole>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserRole>> GetByRoleIdAsync(string roleId);
    Task<UserRole?> GetByUserIdAndRoleIdAsync(string userId, string roleId);
    Task<bool> HasRoleAsync(string userId, string roleName);
    Task<IEnumerable<string>> GetUserRoleNamesAsync(string userId);

    // Role Management
    Task DeactivateUserRoleAsync(string userId, string roleId);

    // UserService Required Methods
    Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);
    Task<IEnumerable<UserRole>> GetUserRolesByOrganizationAsync(string userId, string organizationId);
    Task<IEnumerable<UserRole>> GetUsersByRoleAsync(string roleId);

    // Utility methods
    Task<int> GetCountAsync();
    Task SaveChangesAsync();
} 