using System.ComponentModel.DataAnnotations;

namespace HealthCareAI.Application.DTOs;

// Registration DTOs
public class RegisterRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    public string? OrganizationId { get; set; }
    public string? InvitationToken { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}

public class RegisterResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool RequiresEmailVerification { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

// Login DTOs
public class LoginRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public string? OrganizationId { get; set; }
    public bool RememberMe { get; set; } = false;
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public string? TwoFactorCode { get; set; }
    public string? TrustedDeviceToken { get; set; }
}

public class LoginResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpires { get; set; }
    public UserProfileDto Profile { get; set; } = null!;
    public List<UserRoleDto> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public bool RequiresTwoFactor { get; set; }
    public bool RequiresPasswordChange { get; set; }
    public string? TwoFactorToken { get; set; }
    public string? TrustedDeviceToken { get; set; }
    public DateTime LoginAt { get; set; }
    public string SessionId { get; set; } = string.Empty;
}

// Token DTOs
public class TokenRequestDto
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
    
    public string? DeviceId { get; set; }
}

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpires { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

// User Profile DTOs
public class UserProfileDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string? OrganizationId { get; set; }
    public string? OrganizationName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Dictionary<string, object>? Preferences { get; set; }
    public Dictionary<string, object>? ProfileData { get; set; }
}

public class UserRoleDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string? SectionId { get; set; }
    public string? SectionName { get; set; }
    public bool IsActive { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

// Two-Factor Authentication DTOs
public class TwoFactorRequestDto
{
    [Required(ErrorMessage = "Two-factor token is required")]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
    public string Code { get; set; } = string.Empty;

    public string? DeviceId { get; set; }
    public bool TrustDevice { get; set; } = false;
}

public class TwoFactorSetupDto
{
    public string QrCodeUrl { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public List<string> BackupCodes { get; set; } = new();
    public string SetupToken { get; set; } = string.Empty;
}

// Password Management DTOs
public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm new password is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
    
    public string? OrganizationId { get; set; }
}

public class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Reset token is required")]
    public string ResetToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm new password is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

// Email Verification DTOs
public class EmailVerificationRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}

public class VerifyEmailRequestDto
{
    [Required(ErrorMessage = "Verification token is required")]
    public string VerificationToken { get; set; } = string.Empty;
}

// Session Management DTOs
public class UserSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsTrustedDevice { get; set; }
}

// Logout DTOs
public class LogoutRequestDto
{
    public bool LogoutFromAllDevices { get; set; } = false;
    public string? SessionId { get; set; }
}

// API Response DTOs
public class AuthResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ValidationErrorDto
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
} 