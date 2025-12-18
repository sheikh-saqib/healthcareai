using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class OrganizationAuthPolicy : BaseEntity
{
    public string OrganizationAuthPolicyId { get; set; } = Guid.NewGuid().ToString("N");
    public string OrganizationId { get; set; } = string.Empty;
    
    // Password policy
    public int MinPasswordLength { get; set; } = 8;
    public int MaxPasswordLength { get; set; } = 128;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialChars { get; set; } = true;
    public int PasswordHistoryCount { get; set; } = 5;
    public int MaxPasswordAge { get; set; } = 90; // days
    
    // Lockout policy
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 30;
    public bool EnableProgressiveLockout { get; set; } = true;
    
    // Session policy
    public int SessionTimeoutMinutes { get; set; } = 480; // 8 hours
    public int AbsoluteSessionTimeoutMinutes { get; set; } = 720; // 12 hours
    public bool RequireReauthForSensitiveActions { get; set; } = true;
    public int ReauthTimeoutMinutes { get; set; } = 15;
    
    // Two-factor authentication
    public bool RequireTwoFactor { get; set; } = true;
    public bool AllowTrustedDevices { get; set; } = true;
    public int TrustedDeviceExpiryDays { get; set; } = 30;
    public JsonDocument? AllowedTwoFactorMethods { get; set; } // SMS, Email, TOTP, etc.
    
    // IP and device restrictions
    public JsonDocument? AllowedIpRanges { get; set; }
    public JsonDocument? BlockedIpRanges { get; set; }
    public bool RequireApprovedDevices { get; set; } = false;
    public bool EnableDeviceFingerprinting { get; set; } = true;
    
    // Additional security settings
    public bool EnableConcurrentSessionLimit { get; set; } = true;
    public int MaxConcurrentSessions { get; set; } = 3;
    public bool EnableLocationBasedSecurity { get; set; } = false;
    public bool EnableRiskBasedAuthentication { get; set; } = true;
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
} 