using Dapper;
using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using HealthCareAI.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using OtpNet;

namespace HealthCareAI.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IAccessPermissionRepository _accessPermissionRepository;
    private readonly IRepository<UserPasswordHistory> _passwordHistoryRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<UserService> _logger;
    private readonly IConfiguration _configuration;

    public UserService(
        IRepository<User> userRepository,
        IUserRoleRepository userRoleRepository,
        IAccessPermissionRepository accessPermissionRepository,
        IRepository<UserPasswordHistory> passwordHistoryRepository,
        IDbConnectionFactory connectionFactory,
        ILogger<UserService> logger,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _accessPermissionRepository = accessPermissionRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _connectionFactory = connectionFactory;
        _logger = logger;
        _configuration = configuration;
    }

    // User Management
    public async Task<User?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (Guid.TryParse(userId, out var guidId))
            {
                var user = await _userRepository.GetByIdAsync(guidId);
                return user;
            }

            // If userId is not a valid GUID, search by UserId field
            var users = await _userRepository.GetAllAsync();
            return users.FirstOrDefault(u => u.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAndOrganizationAsync(string email, string? organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                (organizationId == null || u.Role == organizationId)); // Simplified for now
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email and organization: {Email}, {OrganizationId}", email, organizationId);
            return null;
        }
    }

    public async Task<List<User>> GetUsersAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            return users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users with pagination: Page {Page}, Size {PageSize}", page, pageSize);
            return new List<User>();
        }
    }

    public async Task<List<User>> GetUsersByOrganizationAsync(string organizationId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var organizationUsers = users.Where(u => u.Role == organizationId) // Simplified for now
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return organizationUsers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by organization: {OrganizationId}", organizationId);
            return new List<User>();
        }
    }

    // User Creation and Updates
    public async Task<User> CreateUserAsync(User user, string password, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate unique user ID
            user.UserId = Guid.NewGuid().ToString("N");

            // Hash password
            var (hash, salt) = HashPassword(password);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.HashAlgorithm = "PBKDF2";
            user.LastPasswordChangeAt = DateTime.UtcNow;

            // Set default values
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.AccountStatus = "Pending";
            user.IsEmailVerified = false;
            user.IsPhoneVerified = false;
            user.IsTwoFactorEnabled = false;
            user.FailedLoginAttempts = 0;

            if (transaction != null)
            {
                var connection = transaction.Connection ?? throw new InvalidOperationException("Transaction connection is null");

                // Create password history entry
                var passwordHistory = new UserPasswordHistory
                {
                    Id = Guid.NewGuid().ToString("N"),
                    UserPasswordHistoryId = Guid.NewGuid().ToString("N"),
                    UserId = user.UserId,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    HashAlgorithm = "PBKDF2",
                    ChangeReason = "Initial user creation",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create user with password history in single atomic operation
                await UpsertUserWithTransactionAsync(user, connection, transaction, isUpdate: false, passwordHistory: passwordHistory);
                return user;
            }
            else
            {
                throw new InvalidOperationException($"Error creating user: {user.Email}");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        return await UpdateUserAsync(user, null, cancellationToken);
    }

    public async Task<User> UpdateUserAsync(User user, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        try
        {
            user.UpdatedAt = DateTime.UtcNow;

            if (transaction == null)
            {
                // Use repository for non-transactional updates
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                // Use stored procedure for transactional updates
                var connection = transaction.Connection ?? throw new InvalidOperationException("Transaction connection is null");
                await UpdateUserWithTransactionAsync(user, connection, transaction);
            }

            _logger.LogInformation("User updated successfully: {UserId}", user.UserId);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.UserId);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);


            _logger.LogInformation("User deleted successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.AccountStatus = "Disabled";
            user.AccountStatusChangedAt = DateTime.UtcNow;
            user.AccountStatusReason = "User deactivated";
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("User deactivated successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ReactivateUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.AccountStatus = "Active";
            user.AccountStatusChangedAt = DateTime.UtcNow;
            user.AccountStatusReason = "User reactivated";
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("User reactivated successfully: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating user: {UserId}", userId);
            return false;
        }
    }

    // Password Management
    public async Task<bool> ValidatePasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            return VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password for user: {UserId}", user);
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(User user, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            // Hash new password
            var (hash, salt) = HashPassword(newPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
            user.LastPasswordChangeAt = DateTime.UtcNow;
            user.RequirePasswordChange = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsUpdatePassword = true;
            await _userRepository.UpdateAsync(user);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user: {UserId}", user.UserId);
            return false;
        }
    }

    public async Task<bool> IsPasswordValidAsync(string password, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic password validation
            if (string.IsNullOrEmpty(password) || password.Length < 8)
            {
                return false;
            }

            // Check for required character types
            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            // Organization-specific rules could be added here
            if (!string.IsNullOrEmpty(organizationId))
            {
                // Additional validation based on organization policy
                // For now, use standard rules
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password strength");
            return false;
        }
    }

    public async Task<List<string>> GetPasswordHistoryAsync(string userId, int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _passwordHistoryRepository.GetAllAsync();
            return history.Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(count)
                .Select(ph => ph.PasswordHash)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving password history for user: {UserId}", userId);
            return new List<string>();
        }
    }

    // Email Verification
    public async Task<bool> IsEmailVerifiedAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            return user?.IsEmailVerified ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email verification status for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> MarkEmailAsVerifiedAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.IsEmailVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("Email marked as verified for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking email as verified for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateEmailAsync(string userId, string newEmail, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.Email = newEmail.ToLowerInvariant();
            user.IsEmailVerified = false; // Reset verification status
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("Email updated for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email for user: {UserId}", userId);
            return false;
        }
    }

    // Phone Verification
    public async Task<bool> IsPhoneVerifiedAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            return user?.IsPhoneVerified ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking phone verification status for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> MarkPhoneAsVerifiedAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.IsPhoneVerified = true;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("Phone marked as verified for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking phone as verified for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdatePhoneAsync(string userId, string newPhone, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.PrimaryPhone = newPhone;
            user.IsPhoneVerified = false; // Reset verification status
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("Phone updated for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating phone for user: {UserId}", userId);
            return false;
        }
    }

    // Two-Factor Authentication
    public async Task<bool> IsTwoFactorEnabledAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            return user?.IsTwoFactorEnabled ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking 2FA status for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> EnableTwoFactorAsync(string userId, string secretKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.IsTwoFactorEnabled = true;
            user.TwoFactorSecret = secretKey;
            user.TwoFactorEnabledAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("Two-factor authentication enabled for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling 2FA for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DisableTwoFactorAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorEnabledAt = null;
            user.TwoFactorBackupCodes = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("Two-factor authentication disabled for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling 2FA for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<string?> GetTwoFactorSecretAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            return user?.TwoFactorSecret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving 2FA secret for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> VerifyTwoFactorCodeAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                return false;
            }

            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            var isValid = totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying 2FA code for user: {UserId}", userId);
            return false;
        }
    }

    // User Profile
    public async Task<UserProfileDto> GetUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var profile = new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Phone = user.PrimaryPhone,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Status = user.AccountStatus,
                UserType = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                TwoFactorEnabled = user.IsTwoFactorEnabled,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Preferences = user.Preferences != null ? JsonSerializer.Deserialize<Dictionary<string, object>>(user.Preferences) : null
            };

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            // Update user properties
            user.FirstName = profile.FirstName;
            user.LastName = profile.LastName;
            user.PrimaryPhone = profile.Phone;
            user.ProfilePictureUrl = profile.ProfilePictureUrl;
            user.UpdatedAt = DateTime.UtcNow;

            // Update preferences
            if (profile.Preferences != null)
            {
                user.Preferences = JsonSerializer.Serialize(profile.Preferences);
            }

            await _userRepository.UpdateAsync(user);


            _logger.LogInformation("User profile updated: {UserId}", userId);
            return await GetUserProfileAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile: {UserId}", userId);
            throw;
        }
    }

    // User Roles and Permissions
    public async Task<List<UserRole>> GetUserRolesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return (await _userRoleRepository.GetUserRolesAsync(userId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles: {UserId}", userId);
            return new List<UserRole>();
        }
    }

    public async Task<List<UserRole>> GetUserRolesByOrganizationAsync(string userId, string organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            return (await _userRoleRepository.GetUserRolesByOrganizationAsync(userId, organizationId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user roles by organization: {UserId}, {OrganizationId}", userId, organizationId);
            return new List<UserRole>();
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return (await _accessPermissionRepository.GetUserPermissionsAsync(userId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user permissions: {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId, CancellationToken cancellationToken = default)
    {
        try
        {
            return (await _accessPermissionRepository.GetUserPermissionsByOrganizationAsync(userId, organizationId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user permissions by organization: {UserId}, {OrganizationId}", userId, organizationId);
            return new List<string>();
        }
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get user permissions directly - this is more efficient than checking roles individually
            var userPermissions = await _accessPermissionRepository.GetUserPermissionsAsync(userId);
            return userPermissions.Contains(permission);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user permission: {UserId}, {Permission}", userId, permission);
            return false;
        }
    }

    // User Status and Activity
    public async Task<bool> IsUserActiveAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            return user?.AccountStatus == "Active";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user active status: {UserId}", user.UserId);
            return false;
        }
    }

    public async Task<bool> IsUserLockedOutAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            return user?.AccountLockedUntil > DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user lockout status: {UserId}", user.UserId);
            return false;
        }
    }

    public async Task<DateTime?> GetLastLoginAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            return user?.LastLoginAt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving last login for user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            user.LastLoginAt = DateTime.UtcNow;
            user.LastActivityAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", user.UserId);
            return false;
        }
    }

    // User Search and Filtering
    public async Task<List<User>> SearchUsersAsync(string searchTerm, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var filteredUsers = users.Where(u =>
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.FullName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(organizationId))
            {
                filteredUsers = filteredUsers.Where(u => u.Role == organizationId); // Simplified for now
            }

            return filteredUsers.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users: {SearchTerm}", searchTerm);
            return new List<User>();
        }
    }

    public async Task<List<User>> GetUsersByRoleAsync(string roleId, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRoles = await _userRoleRepository.GetUsersByRoleAsync(roleId);
            var userIds = userRoles.Select(ur => ur.UserId).ToList();

            var users = await _userRepository.GetAllAsync();
            var filteredUsers = users.Where(u => userIds.Contains(u.UserId));

            if (!string.IsNullOrEmpty(organizationId))
            {
                filteredUsers = filteredUsers.Where(u => u.Role == organizationId); // Simplified for now
            }

            return filteredUsers.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users by role: {RoleId}", roleId);
            return new List<User>();
        }
    }

    public async Task<bool> EmailExistsAsync(string email, string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var existingUser = users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

            if (existingUser == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(organizationId))
            {
                return existingUser.Role == organizationId; // Simplified for now
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            return false;
        }
    }

    // User Preferences and Settings
    public async Task<Dictionary<string, object>?> GetUserPreferencesAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user?.Preferences == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<Dictionary<string, object>>(user.Preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preferences: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, Dictionary<string, object> preferences, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
            if (user == null)
            {
                return false;
            }

            user.Preferences = JsonSerializer.Serialize(preferences);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);


            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences: {UserId}", userId);
            return false;
        }
    }

    public async Task<object?> GetUserPreferenceAsync(string userId, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId, cancellationToken);
            return preferences?.GetValueOrDefault(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user preference: {UserId}, {Key}", userId, key);
            return null;
        }
    }

    public async Task<bool> SetUserPreferenceAsync(string userId, string key, object value, CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId, cancellationToken) ?? new Dictionary<string, object>();
            preferences[key] = value;

            return await UpdateUserPreferencesAsync(userId, preferences, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user preference: {UserId}, {Key}", userId, key);
            return false;
        }
    }

    // User Statistics
    public async Task<int> GetTotalUsersCountAsync(string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(organizationId))
            {
                return users.Count(u => u.Role == organizationId); // Simplified for now
            }

            return users.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving total users count");
            return 0;
        }
    }

    public async Task<int> GetActiveUsersCountAsync(string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var activeUsers = users.Where(u => u.AccountStatus == "Active");

            if (!string.IsNullOrEmpty(organizationId))
            {
                activeUsers = activeUsers.Where(u => u.Role == organizationId); // Simplified for now
            }

            return activeUsers.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active users count");
            return 0;
        }
    }

    public async Task<int> GetUsersRegisteredTodayAsync(string? organizationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var today = DateTime.UtcNow.Date;
            var registeredToday = users.Where(u => u.CreatedAt.Date == today);

            if (!string.IsNullOrEmpty(organizationId))
            {
                registeredToday = registeredToday.Where(u => u.Role == organizationId); // Simplified for now
            }

            return registeredToday.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users registered today count");
            return 0;
        }
    }

    // Private helper methods
    private static (string hash, string salt) HashPassword(string password)
    {
        // Generate salt
        var salt = GenerateSalt();

        // Hash password with salt using PBKDF2
        using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 100000, HashAlgorithmName.SHA256);
        var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

        return (hash, salt);
    }

    private static string GenerateSalt()
    {
        var salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 100000, HashAlgorithmName.SHA256);
            var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
            return computedHash == hash;
        }
        catch
        {
            return false;
        }
    }

    // Transaction-aware helper methods
    private async Task<string> UpsertUserWithTransactionAsync(User user, IDbConnection connection, IDbTransaction transaction, bool isUpdate = false, UserPasswordHistory? passwordHistory = null)
    {
        var sql = @"SELECT * FROM sp_upsertuserwithistory(
            @UserId, @Email, @Username, @PasswordHash, @PasswordSalt, @HashAlgorithm,
            @FirstName, @LastName, @MiddleName, @PrimaryPhone, @SecondaryPhone, @AlternateEmail,
            @DateOfBirth, @Gender, @Nationality, @PreferredLanguage, @TimeZone,
            @AddressLine1, @AddressLine2, @City, @StateProvince, @PostalCode, @Country,
            @IsEmailVerified, @IsPhoneVerified, @RequirePasswordChange, @LastPasswordChangeAt, @LastLoginAt, @LastActivityAt,
            @FailedLoginAttempts, @LastFailedLoginAt, @AccountLockedUntil, @LockoutReason,
            @IsTwoFactorEnabled, @TwoFactorSecret, @TwoFactorBackupCodes, @TwoFactorEnabledAt,
            @AccountStatus, @AccountStatusChangedAt, @AccountStatusReason,
            @SecurityPreferences, @ForceLogoutAllDevices, @ForceLogoutAfter,
            @ProfilePictureUrl, @Bio, @Preferences, @NotificationSettings,
            @EmergencyContactName, @EmergencyContactPhone, @EmergencyContactRelation,
            @Specialty, @LicenseNumber, @ClinicName, @ClinicAddress, @Role,
            @Id, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
            @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata,
            @PasswordHistoryId, @UserPasswordHistoryId, @ChangeReason, @ChangedByUserId, @IpAddress, @UserAgent,
            @CreatePasswordHistory
        )";

        var parameters = new
        {
            // User parameters
            UserId = user.UserId,
            Email = user.Email,
            Username = user.Username,
            PasswordHash = isUpdate ? null : user.PasswordHash, // Don't update password unless explicitly requested
            PasswordSalt = isUpdate ? null : user.PasswordSalt,
            HashAlgorithm = user.HashAlgorithm,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            PrimaryPhone = user.PrimaryPhone,
            SecondaryPhone = user.SecondaryPhone,
            AlternateEmail = user.AlternateEmail,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Nationality = user.Nationality,
            PreferredLanguage = user.PreferredLanguage,
            TimeZone = user.TimeZone,
            AddressLine1 = user.AddressLine1,
            AddressLine2 = user.AddressLine2,
            City = user.City,
            StateProvince = user.StateProvince,
            PostalCode = user.PostalCode,
            Country = user.Country,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            RequirePasswordChange = user.RequirePasswordChange,
            LastPasswordChangeAt = user.LastPasswordChangeAt,
            LastLoginAt = user.LastLoginAt,
            LastActivityAt = user.LastActivityAt,
            FailedLoginAttempts = user.FailedLoginAttempts,
            LastFailedLoginAt = user.LastFailedLoginAt,
            AccountLockedUntil = user.AccountLockedUntil,
            LockoutReason = user.LockoutReason,
            IsTwoFactorEnabled = user.IsTwoFactorEnabled,
            TwoFactorSecret = user.TwoFactorSecret,
            TwoFactorBackupCodes = user.TwoFactorBackupCodes,
            TwoFactorEnabledAt = user.TwoFactorEnabledAt,
            AccountStatus = user.AccountStatus,
            AccountStatusChangedAt = user.AccountStatusChangedAt,
            AccountStatusReason = user.AccountStatusReason,
            SecurityPreferences = user.SecurityPreferences,
            ForceLogoutAllDevices = user.ForceLogoutAllDevices,
            ForceLogoutAfter = user.ForceLogoutAfter,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Bio = user.Bio,
            Preferences = user.Preferences,
            NotificationSettings = user.NotificationSettings,
            EmergencyContactName = user.EmergencyContactName,
            EmergencyContactPhone = user.EmergencyContactPhone,
            EmergencyContactRelation = user.EmergencyContactRelation,
            Specialty = user.Specialty,
            LicenseNumber = user.LicenseNumber,
            ClinicName = user.ClinicName,
            ClinicAddress = user.ClinicAddress,
            Role = user.Role,
            Id = user.Id,
            CreatedAt = user.CreatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedAt = user.UpdatedAt,
            UpdatedBy = user.UpdatedBy,
            IsDeleted = user.IsDeleted,
            DeletedAt = user.DeletedAt,
            DeletedBy = user.DeletedBy,
            RowVersion = user.RowVersion,
            TenantId = user.TenantId,
            Metadata = user.Metadata,
            
            // Password history parameters
            PasswordHistoryId = passwordHistory?.Id,
            UserPasswordHistoryId = passwordHistory?.UserPasswordHistoryId,
            ChangeReason = passwordHistory?.ChangeReason,
            ChangedByUserId = passwordHistory?.ChangedByUserId,
            IpAddress = passwordHistory?.IpAddress,
            UserAgent = passwordHistory?.UserAgent,
            CreatePasswordHistory = passwordHistory != null
        };

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters, transaction);
        return result?.operation_type ?? "UNKNOWN";
    }

    private async Task CreateUserWithTransactionAsync(User user, IDbConnection connection, IDbTransaction transaction, UserPasswordHistory? passwordHistory = null)
    {
        await UpsertUserWithTransactionAsync(user, connection, transaction, isUpdate: false, passwordHistory: passwordHistory);
    }

    private async Task UpdateUserWithTransactionAsync(User user, IDbConnection connection, IDbTransaction transaction, UserPasswordHistory? passwordHistory = null)
    {
        await UpsertUserWithTransactionAsync(user, connection, transaction, isUpdate: true, passwordHistory: passwordHistory);
    }


}