using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class UserRole : BaseEntity
{
    public string UserRoleId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string AccessRoleId { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty; // Organization, Department, System, Patient, Team
    public string? SectionId { get; set; } // Context-specific ID
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string AssignedByUserId { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public string? Reason { get; set; }
    public JsonDocument? Conditions { get; set; } // Additional conditions/restrictions
    public JsonDocument? Permissions { get; set; } // Override permissions for this specific assignment
    public string? Notes { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public AccessRole AccessRole { get; set; } = null!;
    public User AssignedByUser { get; set; } = null!;
    public Organization? Organization { get; set; } // When Section = "Organization"
    public Department? Department { get; set; } // When Section = "Department"
} 