using HealthCareAI.Application.DTOs;
using HealthCareAI.Domain.Entities;
using System.Data;

namespace HealthCareAI.Application.Interfaces;

public interface IUserService
{
    // User Management
    Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAndOrganizationAsync(string email, string? organizationId, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersByOrganizationAsync(string organizationId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    // User Creation and Updates
    Task<User> CreateUserAsync(User user, string password, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(User user, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ReactivateUserAsync(string userId, CancellationToken cancellationToken = default);

    // Password Management
    Task<bool> ValidatePasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> UpdatePasswordAsync(User user, string newPassword, CancellationToken cancellationToken = default);
    Task<bool> IsPasswordValidAsync(string password, string? organizationId = null, CancellationToken cancellationToken = default);
    Task<List<string>> GetPasswordHistoryAsync(string userId, int count = 10, CancellationToken cancellationToken = default);

    // Email Verification
    Task<bool> IsEmailVerifiedAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkEmailAsVerifiedAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateEmailAsync(string userId, string newEmail, CancellationToken cancellationToken = default);

    // Phone Verification
    Task<bool> IsPhoneVerifiedAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> MarkPhoneAsVerifiedAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdatePhoneAsync(string userId, string newPhone, CancellationToken cancellationToken = default);

    // Two-Factor Authentication
    Task<bool> IsTwoFactorEnabledAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> EnableTwoFactorAsync(string userId, string secretKey, CancellationToken cancellationToken = default);
    Task<bool> DisableTwoFactorAsync(string userId, CancellationToken cancellationToken = default);
    Task<string?> GetTwoFactorSecretAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken cancellationToken = default);

    // User Profile
    Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserProfileDto> UpdateUserProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default);

    // User Roles and Permissions
    Task<List<UserRole>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<UserRole>> GetUserRolesByOrganizationAsync(string userId, string organizationId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(string userId, string permission, string? organizationId = null, CancellationToken cancellationToken = default);

    // User Status and Activity
    Task<bool> IsUserActiveAsync(User userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserLockedOutAsync(User userId, CancellationToken cancellationToken = default);
    Task<DateTime?> GetLastLoginAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateLastLoginAsync(User userId, CancellationToken cancellationToken = default);

    // User Search and Filtering
    Task<List<User>> SearchUsersAsync(string searchTerm, string? organizationId = null, CancellationToken cancellationToken = default);
    Task<List<User>> GetUsersByRoleAsync(string roleId, string? organizationId = null, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, string? organizationId = null, CancellationToken cancellationToken = default);

    // User Preferences and Settings
    Task<Dictionary<string, object>?> GetUserPreferencesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserPreferencesAsync(string userId, Dictionary<string, object> preferences, CancellationToken cancellationToken = default);
    Task<object?> GetUserPreferenceAsync(string userId, string key, CancellationToken cancellationToken = default);
    Task<bool> SetUserPreferenceAsync(string userId, string key, object value, CancellationToken cancellationToken = default);

    // User Statistics
    Task<int> GetTotalUsersCountAsync(string? organizationId = null, CancellationToken cancellationToken = default);
    Task<int> GetActiveUsersCountAsync(string? organizationId = null, CancellationToken cancellationToken = default);
    Task<int> GetUsersRegisteredTodayAsync(string? organizationId = null, CancellationToken cancellationToken = default);
} 