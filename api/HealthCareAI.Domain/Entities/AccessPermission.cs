using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class AccessPermission : BaseEntity
{
    public string AccessPermissionId { get; set; } = Guid.NewGuid().ToString("N");
    public string AccessRoleId { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty; // API, Feature, Page, Data, Report
    public string ResourceIdentifier { get; set; } = string.Empty; // /api/patients, CreatePatient, etc.
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Execute, Export
    public string? Section { get; set; } // Optional section context
    public bool IsAllowed { get; set; } = true;
    public JsonDocument? Conditions { get; set; } // Complex permission conditions
    public int Priority { get; set; } = 0; // For permission conflicts
    public string? Description { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Tags { get; set; } // Comma-separated tags for grouping
    
    // Navigation properties
    public AccessRole AccessRole { get; set; } = null!;
} 