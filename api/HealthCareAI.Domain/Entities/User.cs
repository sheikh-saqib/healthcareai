using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class User : BaseEntity
{
    public string UserId { get; set; } = Guid.NewGuid().ToString("N");
    public string Email { get; set; } = string.Empty;
    public string? Username { get; set; } // Optional username for login
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "PBKDF2"; // Track algorithm used
    
    // Personal information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    
    // Contact information
    public string? PrimaryPhone { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? AlternateEmail { get; set; }
    
    // Personal details
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Nationality { get; set; }
    public string? PreferredLanguage { get; set; } = "en-US";
    public string? TimeZone { get; set; } = "UTC";
    
    // Address information
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Account security
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public bool RequirePasswordChange { get; set; } = false;
    public DateTime? LastPasswordChangeAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
    
    // Account lockout
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastFailedLoginAt { get; set; }
    public DateTime? AccountLockedUntil { get; set; }
    public string? LockoutReason { get; set; }
    
    // Two-factor authentication
    public bool IsTwoFactorEnabled { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public string? TwoFactorBackupCodes { get; set; } // Stored as text in DB
    public DateTime? TwoFactorEnabledAt { get; set; }
    
    // Account status
    public string AccountStatus { get; set; } = "Active"; // Active, Suspended, Locked, Pending, Disabled
    public DateTime? AccountStatusChangedAt { get; set; }
    public string? AccountStatusReason { get; set; }
    
    // Security preferences
    public string? SecurityPreferences { get; set; } // Stored as text in DB
    public bool ForceLogoutAllDevices { get; set; } = false;
    public DateTime? ForceLogoutAfter { get; set; }
    
    // Profile information
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public string? Preferences { get; set; } // Stored as text in DB
    public string? NotificationSettings { get; set; } // Stored as text in DB
    
    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    
    // Legacy fields for backwards compatibility (will be removed eventually)
    public string? Specialty { get; set; }
    public string? LicenseNumber { get; set; }
    public string? ClinicName { get; set; }
    public string? ClinicAddress { get; set; }
    public string Role { get; set; } = "user";

    // Navigation properties - commented out until entities are added to DbContext
    // public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    // public ICollection<DoctorProfile> DoctorProfiles { get; set; } = new List<DoctorProfile>();
    // public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    // public ICollection<VerificationToken> VerificationTokens { get; set; } = new List<VerificationToken>();
    // public ICollection<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
    // public ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    // public ICollection<UserPasswordHistory> PasswordHistory { get; set; } = new List<UserPasswordHistory>();

    // Role assignments
    // public ICollection<UserRole> AssignedRoles { get; set; } = new List<UserRole>();
    // public ICollection<UserRole> RoleAssignments { get; set; } = new List<UserRole>();

    // Departments headed
    // public ICollection<Department> DepartmentsHeaded { get; set; } = new List<Department>();

    public bool IsUpdatePassword { get; set; } = false;
    
    // Password history tracking (used when IsUpdatePassword = true)
    public string? ChangeReason { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
} 