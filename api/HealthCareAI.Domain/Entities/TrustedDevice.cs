using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class TrustedDevice : BaseEntity
{
    public string TrustedDeviceId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // Web, Mobile, Desktop
    public JsonDocument? DeviceFingerprint { get; set; }
    public DateTime TrustedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public string? UserAgent { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
} 