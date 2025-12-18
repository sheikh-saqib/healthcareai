using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class OrganizationSetting : BaseEntity
{
    public string OrganizationSettingId { get; set; } = Guid.NewGuid().ToString("N");
    public string OrganizationId { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; } = false;
    public bool IsSystem { get; set; } = false;
    public string? Category { get; set; }
    public string? DataType { get; set; } = "string"; // string, int, bool, json, etc.
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
} 