using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HealthCareAI.Domain.Common;

public abstract class BaseEntity
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N"); // UUID without dashes
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    
    // Optimistic concurrency control
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Tenant isolation
    public string? TenantId { get; set; }
    
    // Metadata for extensibility
    public JsonDocument? Metadata { get; set; }
} 