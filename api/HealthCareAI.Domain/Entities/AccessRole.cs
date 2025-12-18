using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class AccessRole : BaseEntity
{
    public string AccessRoleId { get; set; } = Guid.NewGuid().ToString("N");
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty; // SUPER_ADMIN, ORG_ADMIN, etc.
    public string? Description { get; set; }
    public int Priority { get; set; } = 0; // For role hierarchy
    public bool IsSystemRole { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? ParentRoleId { get; set; } // For role hierarchy
    public JsonDocument? DefaultPermissions { get; set; }
    public string? Category { get; set; } // Admin, Medical, Support, etc.
    public string? Color { get; set; } // For UI display
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public AccessRole? ParentRole { get; set; }
    public ICollection<AccessRole> ChildRoles { get; set; } = new List<AccessRole>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<AccessPermission> AccessPermissions { get; set; } = new List<AccessPermission>();
} 