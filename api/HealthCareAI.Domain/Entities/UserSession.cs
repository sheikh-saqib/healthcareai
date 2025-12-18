using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class UserSession : BaseEntity
{
    public string UserSessionId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty; // Hashed session identifier
    public string RefreshToken { get; set; } = string.Empty; // For token refresh
    public string JwtTokenId { get; set; } = string.Empty; // JTI claim from JWT
    
    // Session timing
    public DateTime ExpiresAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Device/client information
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // Web, Mobile, Desktop
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? Location { get; set; }
    public JsonDocument? DeviceFingerprint { get; set; }
    
    // Security flags
    public bool IsTrustedDevice { get; set; } = false;
    public bool RequireTwoFactor { get; set; } = true;
    public string? LoginMethod { get; set; } = "Password"; // Password, TwoFactor, SSO, Biometric
    
    // Context information
    public string? OrganizationId { get; set; } // Current organization context
    public string? SelectedRoleId { get; set; } // Current role context
    public JsonDocument? SessionData { get; set; } // Additional session data
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Organization? Organization { get; set; }
} 