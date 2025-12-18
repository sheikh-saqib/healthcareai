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
using OtpNet;
using QRCoder;

namespace HealthCareAI.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IAccessPermissionRepository _accessPermissionRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        IUserService userService,
        ITokenService tokenService,
        IUserSessionRepository userSessionRepository,
        IVerificationTokenRepository verificationTokenRepository,
        IUserRoleRepository userRoleRepository,
        IAccessPermissionRepository accessPermissionRepository,
        IDbConnectionFactory connectionFactory,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _userService = userService;
        _tokenService = tokenService;
        _userSessionRepository = userSessionRepository;
        _verificationTokenRepository = verificationTokenRepository;
        _userRoleRepository = userRoleRepository;
        _accessPermissionRepository = accessPermissionRepository;
        _connectionFactory = connectionFactory;
        _logger = logger;
        _configuration = configuration;
    }

    // User Registration
    public async Task<AuthResponseDto<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _userService.GetUserByEmailAsync(request.Email, cancellationToken);
            if (existingUser != null)
            {
                return new AuthResponseDto<RegisterResponseDto>
                {
                    Success = false,
                    Message = "Email address is already registered",
                    Errors = new List<string> { "Email address is already in use" }
                };
            }

            // Validate password strength
            if (!await _userService.IsPasswordValidAsync(request.Password, request.OrganizationId, cancellationToken))
            {
                return new AuthResponseDto<RegisterResponseDto>
                {
                    Success = false,
                    Message = "Password does not meet security requirements",
                    Errors = new List<string> { "Password must be at least 8 characters with uppercase, lowercase, number, and special character" }
                };
            }

            // Create user
            var user = new User
            {
                Email = request.Email.ToLowerInvariant(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PrimaryPhone = request.Phone,
                AccountStatus = "Pending", // Pending until email verification
                RequirePasswordChange = false,
                IsEmailVerified = false,
                IsPhoneVerified = false,
                IsTwoFactorEnabled = false,
                Role = "User", // Default role
                PreferredLanguage = "en-US",
                TimeZone = "UTC",
                CreatedAt = DateTime.UtcNow
            };

            // Start transaction for complete registration process
            using var connection = await _connectionFactory.CreateConnectionAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var createdUser = await _userService.CreateUserAsync(user, request.Password, transaction, cancellationToken);
                _logger.LogInformation("User creation took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // Generate email verification token
                stopwatch.Restart();
                var verificationToken = await _tokenService.GenerateEmailVerificationTokenAsync(
                    createdUser.UserId,
                    createdUser.Email,
                    transaction,
                    cancellationToken);
                _logger.LogInformation("Token generation took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

                // Commit all registration operations atomically
                transaction.Commit();

                var response = new RegisterResponseDto
                {
                    UserId = createdUser.UserId,
                    Email = createdUser.Email,
                    FullName = createdUser.FullName,
                    RequiresEmailVerification = true,
                    VerificationToken = verificationToken,
                    CreatedAt = createdUser.CreatedAt,
                    Message = "Registration successful. Please verify your email address to activate your account."
                };

                return new AuthResponseDto<RegisterResponseDto>
                {
                    Success = true,
                    Message = "Registration successful",
                    Data = response
                };
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            return new AuthResponseDto<RegisterResponseDto>
            {
                Success = false,
                Message = "An error occurred during registration",
                Errors = ["Registration failed. Please try again."]
            };
        }
    }

    public async Task<AuthResponseDto<bool>> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting email verification for token: {Token}", request.VerificationToken[..8] + "...");

            // Get the verification token from the database
            var tokenRecord = await _verificationTokenRepository.GetByTokenAndTypeAsync(request.VerificationToken, "EmailVerification");

            if (tokenRecord == null || tokenRecord.ExpiresAt <= DateTime.UtcNow || tokenRecord.IsUsed)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or expired verification token",
                    Errors = new List<string> { "The verification token is invalid or has expired" }
                };
            }

            var user = await _userService.GetUserByIdAsync(tokenRecord.UserId, cancellationToken);
            if (user == null)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "User account not found" }
                };
            }

            // Mark email as verified and activate account
            await _userService.MarkEmailAsVerifiedAsync(user.UserId, cancellationToken);

            // Update account status to Active
            user.AccountStatus = "Active";
            user.AccountStatusChangedAt = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user, cancellationToken);

            // Mark token as used
            tokenRecord.IsUsed = true;
            tokenRecord.UpdatedAt = DateTime.UtcNow;
            await _verificationTokenRepository.UpdateAsync(tokenRecord);

            _logger.LogInformation("Email verification completed for user: {UserId}", user.UserId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Email verified successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred during email verification",
                Errors = new List<string> { "Email verification failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> ResendEmailVerificationAsync(EmailVerificationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "User account not found" }
                };
            }

            if (user.IsEmailVerified)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Email is already verified",
                    Errors = new List<string> { "Your email address is already verified" }
                };
            }

            var verificationToken = await _tokenService.GenerateEmailVerificationTokenAsync(
                user.UserId,
                user.Email,
                null,
                cancellationToken);

            _logger.LogInformation("Email verification token resent for user: {UserId}", user.UserId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Verification email sent successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email verification");
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while resending verification email",
                Errors = new List<string> { "Failed to resend verification email. Please try again." }
            };
        }
    }

    // User Login
    public async Task<AuthResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userService.GetUserByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = ["Invalid credentials"]
                };
            }

            // Check if account is locked
            if (await _userService.IsUserLockedOutAsync(user, cancellationToken))
            {
                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Account is temporarily locked",
                    Errors = ["Account is locked due to multiple failed login attempts"]
                };
            }

            // Check if account is active
            if (!await _userService.IsUserActiveAsync(user, cancellationToken))
            {
                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Account is not active",
                    Errors = ["Account is not active. Please contact support."]
                };
            }

            // Validate password
            if (!await _userService.ValidatePasswordAsync(user, request.Password, cancellationToken))
            {
                // Increment failed login attempts
                user.FailedLoginAttempts++;
                user.LastFailedLoginAt = DateTime.UtcNow;

                // Lock account if too many failed attempts
                if (user.FailedLoginAttempts >= 5)
                {
                    user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(30);
                    user.LockoutReason = "Too many failed login attempts";
                }

                await _userService.UpdateUserAsync(user, cancellationToken);

                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid email or password",
                    Errors = ["Invalid credentials"]
                };
            }

            // Check if two-factor authentication is required CHECK TODO
            if (user.IsTwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode))
            {
                // Generate 2FA token
                var twoFactorToken = await _tokenService.GenerateTwoFactorTokenAsync(user.UserId, cancellationToken);

                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Two-factor authentication required",
                    Data = new LoginResponseDto
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        RequiresTwoFactor = true,
                        TwoFactorToken = twoFactorToken
                    }
                };
            }

            // Verify 2FA code if provided CHECK TODO
            if (user.IsTwoFactorEnabled && !string.IsNullOrEmpty(request.TwoFactorCode))
            {
                if (!await _userService.VerifyTwoFactorCodeAsync(user.UserId, request.TwoFactorCode, cancellationToken))
                {
                    return new AuthResponseDto<LoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid two-factor authentication code",
                        Errors = new List<string> { "Invalid 2FA code" }
                    };
                }
            }

            // Reset failed login attempts on successful login
            user.FailedLoginAttempts = 0;
            user.LastFailedLoginAt = null;
            user.AccountLockedUntil = null;
            user.LockoutReason = null;
            await _userService.UpdateLastLoginAsync(user, cancellationToken);

            // Generate tokens
            var userRoles = await _userRoleRepository.GetByUserIdAsync(user.UserId);
            var roleIds = userRoles.Select(r => r.AccessRoleId).ToList();
            var permissions = roleIds.Any() ? (await _accessPermissionRepository.GetPermissionsForRolesAsync(roleIds)).ToList() : new List<string>();

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, userRoles.ToList(), permissions.ToList(), cancellationToken);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(cancellationToken);

            // Create session
            var session = new UserSession
            {
                UserSessionId = Guid.NewGuid().ToString("N"),
                UserId = user.UserId,
                RefreshToken = refreshToken,
                DeviceId = request.DeviceId ?? "Unknown",
                DeviceName = request.DeviceName ?? "Unknown Device",
                DeviceType = request.DeviceType ?? "Unknown",
                IpAddress = ipAddress,
                UserAgent = userAgent,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                IsTrustedDevice = false,
                CreatedAt = DateTime.UtcNow,

                // Ensure navigation properties are null to prevent Dapper serialization issues
                User = null,
                Organization = null
            };

            await _userSessionRepository.CreateAsync(session);

            var profile = await _userService.GetUserProfileAsync(user.UserId, cancellationToken);
            var roleDtos = userRoles.Select(r => new UserRoleDto
            {
                RoleId = r.AccessRoleId,
                RoleName = r.AccessRole.RoleName,
                Section = r.Section,
                SectionId = r.SectionId,
                IsActive = r.IsActive,
                AssignedAt = r.AssignedAt,
                ExpiresAt = r.ExpiresAt
            }).ToList();

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpires = DateTime.UtcNow.AddMinutes(15), // From config
                Profile = profile,
                Roles = roleDtos,
                Permissions = permissions,
                RequiresTwoFactor = false,
                RequiresPasswordChange = user.RequirePasswordChange,
                LoginAt = DateTime.UtcNow,
                SessionId = session.UserSessionId
            };

            _logger.LogInformation("Login successful for user: {UserId}", user.UserId);

            return new AuthResponseDto<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", request.Email);
            return new AuthResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = new List<string> { "Login failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<LoginResponseDto>> CompleteLoginAsync(string twoFactorToken, string code, string? deviceId, bool trustDevice, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify 2FA token and get user ID
            var tokenValidation = await _verificationTokenRepository.GetByTokenAndTypeAsync(twoFactorToken, "TwoFactorAuthentication");

            if (tokenValidation == null || tokenValidation.ExpiresAt <= DateTime.UtcNow || tokenValidation.IsUsed)
            {
                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired two-factor token",
                    Errors = new List<string> { "Two-factor authentication session expired" }
                };
            }

            var user = await _userService.GetUserByIdAsync(tokenValidation.UserId, cancellationToken);
            if (user == null)
            {
                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "User account not found" }
                };
            }

            // Verify 2FA code
            if (!await _userService.VerifyTwoFactorCodeAsync(user.UserId, code, cancellationToken))
            {
                return new AuthResponseDto<LoginResponseDto>
                {
                    Success = false,
                    Message = "Invalid two-factor authentication code",
                    Errors = new List<string> { "Invalid 2FA code" }
                };
            }

            // Mark 2FA token as used
            tokenValidation.IsUsed = true;
            tokenValidation.UpdatedAt = DateTime.UtcNow;
            await _verificationTokenRepository.UpdateAsync(tokenValidation);

            // Complete login process similar to regular login
            var userRoles = await _userRoleRepository.GetByUserIdAsync(user.UserId);
            var roleIds = userRoles.Select(r => r.AccessRoleId).ToList();
            var permissions = roleIds.Any() ? (await _accessPermissionRepository.GetPermissionsForRolesAsync(roleIds)).ToList() : new List<string>();

            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, userRoles.ToList(), permissions.ToList(), cancellationToken);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(cancellationToken);

            // Create session
            var session = new UserSession
            {
                UserSessionId = Guid.NewGuid().ToString("N"),
                UserId = user.UserId,
                RefreshToken = refreshToken,
                DeviceId = deviceId ?? "Unknown",
                DeviceName = "Unknown Device",
                DeviceType = "Unknown",
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                IsTrustedDevice = trustDevice,
                CreatedAt = DateTime.UtcNow,

                // Ensure navigation properties are null to prevent Dapper serialization issues
                User = null,
                Organization = null
            };

            await _userSessionRepository.CreateAsync(session);

            // Generate trusted device token if requested
            string? trustedDeviceToken = null;
            if (trustDevice && !string.IsNullOrEmpty(deviceId))
            {
                trustedDeviceToken = await _tokenService.GenerateTrustedDeviceTokenAsync(user.UserId, deviceId, cancellationToken);
            }

            var profile = await _userService.GetUserProfileAsync(user.UserId, cancellationToken);
            var roleDtos = userRoles.Select(r => new UserRoleDto
            {
                RoleId = r.AccessRoleId,
                RoleName = r.AccessRole.RoleName,
                Section = r.Section,
                SectionId = r.SectionId,
                IsActive = r.IsActive,
                AssignedAt = r.AssignedAt,
                ExpiresAt = r.ExpiresAt
            }).ToList();

            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpires = DateTime.UtcNow.AddMinutes(15),
                Profile = profile,
                Roles = roleDtos,
                Permissions = permissions,
                RequiresTwoFactor = false,
                RequiresPasswordChange = user.RequirePasswordChange,
                TrustedDeviceToken = trustedDeviceToken,
                LoginAt = DateTime.UtcNow,
                SessionId = session.UserSessionId
            };

            _logger.LogInformation("Two-factor login completed for user: {UserId}", user.UserId);

            return new AuthResponseDto<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during two-factor login completion");
            return new AuthResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "An error occurred during login",
                Errors = new List<string> { "Login failed. Please try again." }
            };
        }
    }

    // For now, I'll implement simplified versions of the remaining methods to get compilation working
    // These can be expanded later with full functionality

    public async Task<AuthResponseDto<TokenResponseDto>> RefreshTokenAsync(TokenRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find the session with this refresh token
            var session = await _userSessionRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (session == null || !session.IsActive || session.ExpiresAt <= DateTime.UtcNow)
            {
                return new AuthResponseDto<TokenResponseDto>
                {
                    Success = false,
                    Message = "Invalid or expired refresh token",
                    Errors = ["The refresh token is invalid or has expired"]
                };
            }

            // Get the user
            var user = await _userService.GetUserByIdAsync(session.UserId, cancellationToken);
            if (user == null || user.AccountStatus != "Active")
            {
                return new AuthResponseDto<TokenResponseDto>
                {
                    Success = false,
                    Message = "User not found or inactive",
                    Errors = ["The user associated with this token is not valid"]
                };
            }

            // Get user roles and permissions for token generation
            var userRoles = await _userRoleRepository.GetUserRolesAsync(user.UserId);
            var permissions = await _accessPermissionRepository.GetUserPermissionsAsync(user.UserId);

            // Generate new tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, userRoles.ToList(), permissions.ToList(), cancellationToken);
            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(cancellationToken);

            // Update the session with new refresh token and extend expiry
            session.RefreshToken = newRefreshToken;
            session.LastAccessedAt = DateTime.UtcNow;
            session.ExpiresAt = DateTime.UtcNow.AddDays(30); // Extend for 30 days
            session.IpAddress = ipAddress;
            session.UserAgent = userAgent;
            session.UpdatedAt = DateTime.UtcNow;

            await _userSessionRepository.CreateAsync(session);

            // Get token expiration
            var tokenExpiration = await _tokenService.GetTokenExpirationAsync(accessToken, cancellationToken) ?? DateTime.UtcNow.AddMinutes(15);

            _logger.LogInformation("Successfully refreshed token for user: {UserId}", user.UserId);

            return new AuthResponseDto<TokenResponseDto>
            {
                Success = true,
                Message = "Token refreshed successfully",
                Data = new TokenResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    TokenExpires = tokenExpiration,
                    TokenType = "Bearer"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return new AuthResponseDto<TokenResponseDto>
            {
                Success = false,
                Message = "An error occurred during token refresh",
                Errors = new List<string> { "Token refresh failed. Please login again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> LogoutAsync(string userId, LogoutRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to logout user: {UserId}, LogoutFromAllDevices: {LogoutFromAllDevices}", userId, request.LogoutFromAllDevices);

            if (request.LogoutFromAllDevices)
            {
                // Logout from all devices
                var deactivatedCount = await _userSessionRepository.DeactivateAllByUserIdAsync(userId, null);
                _logger.LogInformation("Deactivated {Count} sessions for user: {UserId}", deactivatedCount, userId);
            }
            else if (!string.IsNullOrEmpty(request.SessionId))
            {
                // Logout from specific session
                var success = await _userSessionRepository.DeactivateAsync(request.SessionId);
                if (!success)
                {
                    _logger.LogWarning("Failed to deactivate session: {SessionId} for user: {UserId}", request.SessionId, userId);
                    return new AuthResponseDto<bool>
                    {
                        Success = false,
                        Message = "Session not found or already inactive",
                        Errors = new List<string> { "The specified session could not be found or is already inactive" }
                    };
                }
                _logger.LogInformation("Successfully deactivated session: {SessionId} for user: {UserId}", request.SessionId, userId);
            }
            else
            {
                // If no specific session ID provided, deactivate all active sessions (default behavior)
                var deactivatedCount = await _userSessionRepository.DeactivateAllByUserIdAsync(userId, null);
                _logger.LogInformation("Deactivated {Count} sessions for user: {UserId} (no specific session provided)", deactivatedCount, userId);
            }

            // Update user's last activity time (since we don't have LastLogoutAt property)
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                user.LastActivityAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUserAsync(user, cancellationToken);
            }

            _logger.LogInformation("Successfully logged out user: {UserId}", userId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Logged out successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred during logout",
                Errors = new List<string> { "Logout failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to revoke refresh token: {RefreshToken}", refreshToken[..8] + "...");

            // Find the session with this refresh token
            var session = await _userSessionRepository.GetByRefreshTokenAsync(refreshToken);
            if (session == null)
            {
                _logger.LogWarning("Refresh token not found: {RefreshToken}", refreshToken[..8] + "...");
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Refresh token not found",
                    Errors = new List<string> { "The specified refresh token could not be found" }
                };
            }

            // Deactivate the session
            var success = await _userSessionRepository.DeactivateAsync(session.Id);
            if (!success)
            {
                _logger.LogWarning("Failed to deactivate session for refresh token: {RefreshToken}", refreshToken[..8] + "...");
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to revoke token",
                    Errors = new List<string> { "The token could not be revoked at this time" }
                };
            }

            _logger.LogInformation("Successfully revoked refresh token for user: {UserId}, session: {SessionId}", session.UserId, session.UserSessionId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Token revoked successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking refresh token: {RefreshToken}", refreshToken[..8] + "...");
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while revoking the token",
                Errors = new List<string> { "Token revocation failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> RevokeAllTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Attempting to revoke all tokens for user: {UserId}", userId);

            // Verify user exists
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for token revocation: {UserId}", userId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "The specified user could not be found" }
                };
            }

            // Deactivate all active sessions for this user
            var deactivatedCount = await _userSessionRepository.DeactivateAllByUserIdAsync(userId, null);

            _logger.LogInformation("Successfully revoked {Count} tokens for user: {UserId}", deactivatedCount, userId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = $"Successfully revoked {deactivatedCount} tokens",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while revoking tokens",
                Errors = new List<string> { "Token revocation failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get the user
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
           
            if (user == null)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = ["The specified user could not be found"]
                };
            }

            // Verify current password
            var isCurrentPasswordValid = await _userService.ValidatePasswordAsync(user, request.CurrentPassword, cancellationToken);
            if (!isCurrentPasswordValid)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid current password",
                    Errors = ["The current password provided is incorrect"]
                };
            }

            // Validate new password strength (basic validation)
            if (request.NewPassword.Length < 8)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Password too weak",
                    Errors = ["Password must be at least 8 characters long"]
                };
            }

            // Check if new password is different from current password
            if (await _userService.ValidatePasswordAsync(user, request.NewPassword, cancellationToken))
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "New password cannot be the same as current password",
                    Errors = ["Please choose a different password"]
                };
            }

            // Update password
            var passwordUpdateSuccess = await _userService.UpdatePasswordAsync(user, request.NewPassword, cancellationToken);
            if (!passwordUpdateSuccess)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to update password",
                    Errors = ["The password could not be updated at this time"]
                };
            }

            // Revoke all existing sessions to force re-login with new password
            await _userSessionRepository.DeactivateAllByUserIdAsync(userId, null);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Password changed successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while changing the password",
                Errors = ["Password change failed. Please try again."]
            };
        }
    }

    public async Task<AuthResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email (and organization if provided)
            var user = await _userService.GetUserByEmailAndOrganizationAsync(request.Email, request.OrganizationId, cancellationToken);

            // Always return success to prevent email enumeration attacks
            // Don't reveal whether the email exists or not
            if (user == null || user.AccountStatus != "Active")
            {
                // Still return success to prevent enumeration
                return new AuthResponseDto<bool>
                {
                    Success = true,
                    Message = "If an account with that email exists, a password reset link has been sent",
                    Data = true
                };
            }

            // Generate password reset token
            var resetToken = await _tokenService.GeneratePasswordResetTokenAsync(user.UserId, user.Email, cancellationToken);

            // TODO: Implement email service to send reset link
            // await _emailService.SendPasswordResetEmailAsync(user.Email, user.FirstName, resetToken);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "If an account with that email exists, a password reset link has been sent",
                Data = true
            };
        }
        catch (Exception ex)
        {
            // Still return success to prevent revealing internal errors
            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "If an account with that email exists, a password reset link has been sent",
                Data = true
            };
        }
    }

    public async Task<AuthResponseDto<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing password reset with token: {Token}", request.ResetToken[..8] + "...");

            // Find the verification token
            var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(request.ResetToken, "PasswordReset");

            if (verificationToken == null)
            {
                _logger.LogWarning("Invalid password reset token: {Token}", request.ResetToken[..8] + "...");
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or expired reset token",
                    Errors = new List<string> { "The password reset token is invalid or has expired" }
                };
            }

            // Verify the token is still valid
            var isTokenValid = await _tokenService.VerifyPasswordResetTokenAsync(request.ResetToken, verificationToken.Email!, cancellationToken);
            if (!isTokenValid)
            {
                _logger.LogWarning("Expired or invalid password reset token: {Token}", request.ResetToken[..8] + "...");
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid or expired reset token",
                    Errors = new List<string> { "The password reset token is invalid or has expired" }
                };
            }

            // Get the user
            var user = await _userService.GetUserByIdAsync(verificationToken.UserId, cancellationToken);
            if (user == null || user.AccountStatus != "Active")
            {
                _logger.LogWarning("User not found or inactive for password reset: {UserId}", verificationToken.UserId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found or inactive",
                    Errors = new List<string> { "The user account is not valid" }
                };
            }

            // Validate new password strength
            if (request.NewPassword.Length < 8)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Password too weak",
                    Errors = new List<string> { "Password must be at least 8 characters long" }
                };
            }

            // Update the password
            var passwordUpdateSuccess = await _userService.UpdatePasswordAsync(user, request.NewPassword, cancellationToken);
            if (!passwordUpdateSuccess)
            {
                _logger.LogError("Failed to update password during reset for user: {UserId}", user.UserId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Failed to reset password",
                    Errors = new List<string> { "The password could not be reset at this time" }
                };
            }

            // Mark the token as used
            await _verificationTokenRepository.MarkAsUsedAsync(request.ResetToken);

            // Revoke all existing sessions to force re-login
            await _userSessionRepository.DeactivateAllByUserIdAsync(user.UserId, null);

            _logger.LogInformation("Successfully reset password for user: {UserId}", user.UserId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Password reset successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token: {Token}", request.ResetToken[..8] + "...");
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while resetting the password",
                Errors = new List<string> { "Password reset failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<TwoFactorSetupDto>> SetupTwoFactorAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Setting up two-factor authentication for user: {UserId}", userId);

            // Get the user
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for 2FA setup: {UserId}", userId);
                return new AuthResponseDto<TwoFactorSetupDto>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "The specified user could not be found" }
                };
            }

            // Check if 2FA is already enabled
            if (user.IsTwoFactorEnabled)
            {
                return new AuthResponseDto<TwoFactorSetupDto>
                {
                    Success = false,
                    Message = "Two-factor authentication is already enabled",
                    Errors = new List<string> { "2FA is already enabled for this account" }
                };
            }

            // Generate a secret key for TOTP
            var secretKey = KeyGeneration.GenerateRandomKey(20);
            var base32Secret = Base32Encoding.ToString(secretKey);

            // Create TOTP instance
            var totp = new Totp(secretKey);

            // Generate QR code data
            var qrCodeData = $"otpauth://totp/{Uri.EscapeDataString($"HealthCareAI:{user.Email}")}?secret={base32Secret}&issuer={Uri.EscapeDataString("HealthCareAI")}";

            // Generate QR code image
            var qrGenerator = new QRCodeGenerator();
            var qrCodeInfo = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new Base64QRCode(qrCodeInfo);
            var qrCodeImageBase64 = qrCode.GetGraphic(20);
            var qrCodeUrl = $"data:image/png;base64,{qrCodeImageBase64}";

            // Generate backup codes
            var backupCodes = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                backupCodes.Add(GenerateBackupCode());
            }

            // Generate setup token for verification
            var setupToken = await _tokenService.GenerateTwoFactorTokenAsync(userId, cancellationToken);

            // Store the secret temporarily (in production, you might want to encrypt this)
            user.TwoFactorSecret = base32Secret;
            user.UpdatedAt = DateTime.UtcNow;
            await _userService.UpdateUserAsync(user, cancellationToken);

            _logger.LogInformation("Two-factor authentication setup prepared for user: {UserId}", userId);

            return new AuthResponseDto<TwoFactorSetupDto>
            {
                Success = true,
                Message = "Two-factor authentication setup prepared",
                Data = new TwoFactorSetupDto
                {
                    QrCodeUrl = qrCodeUrl,
                    SecretKey = base32Secret,
                    BackupCodes = backupCodes,
                    SetupToken = setupToken
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up two-factor authentication for user: {UserId}", userId);
            return new AuthResponseDto<TwoFactorSetupDto>
            {
                Success = false,
                Message = "An error occurred while setting up two-factor authentication",
                Errors = new List<string> { "2FA setup failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> EnableTwoFactorAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Enabling two-factor authentication for user: {UserId}", userId);

            // Get the user
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for 2FA enable: {UserId}", userId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "The specified user could not be found" }
                };
            }

            // Check if 2FA is already enabled
            if (user.IsTwoFactorEnabled)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Two-factor authentication is already enabled",
                    Errors = new List<string> { "2FA is already enabled for this account" }
                };
            }

            // Check if secret key exists
            if (string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Two-factor authentication not set up",
                    Errors = new List<string> { "Please set up 2FA first" }
                };
            }

            // Verify the TOTP code
            var secretKeyBytes = Base32Encoding.ToBytes(user.TwoFactorSecret);
            var totp = new Totp(secretKeyBytes);
            var isValidCode = totp.VerifyTotp(code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValidCode)
            {
                _logger.LogWarning("Invalid 2FA code provided for user: {UserId}", userId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid verification code",
                    Errors = new List<string> { "The verification code is incorrect or has expired" }
                };
            }

            // Enable 2FA for the user
            user.IsTwoFactorEnabled = true;
            user.TwoFactorEnabledAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _userService.UpdateUserAsync(user, cancellationToken);

            _logger.LogInformation("Two-factor authentication enabled for user: {UserId}", userId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Two-factor authentication enabled successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling two-factor authentication for user: {UserId}", userId);
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while enabling two-factor authentication",
                Errors = new List<string> { "2FA enable failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<bool>> DisableTwoFactorAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Disabling two-factor authentication for user: {UserId}", userId);

            // Get the user
            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("User not found for 2FA disable: {UserId}", userId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not found",
                    Errors = new List<string> { "The specified user could not be found" }
                };
            }

            // Check if 2FA is enabled
            if (!user.IsTwoFactorEnabled)
            {
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Two-factor authentication is not enabled",
                    Errors = new List<string> { "2FA is not enabled for this account" }
                };
            }

            // Verify the password
            var isPasswordValid = await _userService.ValidatePasswordAsync(user, password, cancellationToken);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Invalid password provided for 2FA disable for user: {UserId}", userId);
                return new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Invalid password",
                    Errors = new List<string> { "The password provided is incorrect" }
                };
            }

            // Disable 2FA for the user
            user.IsTwoFactorEnabled = false;
            user.TwoFactorSecret = null; // Clear the secret key
            user.TwoFactorEnabledAt = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userService.UpdateUserAsync(user, cancellationToken);

            // Optionally, you might want to invalidate all sessions to force re-login
            // await _userSessionRepository.DeactivateAllByUserIdAsync(userId, null);

            _logger.LogInformation("Two-factor authentication disabled for user: {UserId}", userId);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = "Two-factor authentication disabled successfully",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling two-factor authentication for user: {UserId}", userId);
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while disabling two-factor authentication",
                Errors = new List<string> { "2FA disable failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var profile = await _userService.GetUserProfileAsync(userId, cancellationToken);

            return new AuthResponseDto<UserProfileDto>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = profile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user: {UserId}", userId);
            return new AuthResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "An error occurred while retrieving profile",
                Errors = new List<string> { "Profile retrieval failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<UserProfileDto>> UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        try
        {
            var updatedProfile = await _userService.UpdateUserProfileAsync(userId, profile, cancellationToken);

            return new AuthResponseDto<UserProfileDto>
            {
                Success = true,
                Message = "Profile updated successfully",
                Data = updatedProfile
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", userId);
            return new AuthResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "An error occurred while updating profile",
                Errors = new List<string> { "Profile update failed. Please try again." }
            };
        }
    }

    public async Task<AuthResponseDto<List<UserSessionDto>>> GetActiveSessionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Simplified implementation
        return new AuthResponseDto<List<UserSessionDto>>
        {
            Success = false,
            Message = "Not implemented",
            Errors = new List<string> { "Method not implemented" }
        };
    }

    public async Task<AuthResponseDto<bool>> TerminateSessionAsync(string userId, string sessionId, CancellationToken cancellationToken = default)
    {
        // Simplified implementation
        return new AuthResponseDto<bool>
        {
            Success = false,
            Message = "Not implemented",
            Errors = new List<string> { "Method not implemented" }
        };
    }

    public async Task<AuthResponseDto<bool>> TerminateAllSessionsAsync(string userId, string? currentSessionId = null, CancellationToken cancellationToken = default)
    {
        // Simplified implementation
        return new AuthResponseDto<bool>
        {
            Success = false,
            Message = "Not implemented",
            Errors = new List<string> { "Method not implemented" }
        };
    }

    public async Task<AuthResponseDto<bool>> DeactivateAccountAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        // Simplified implementation
        return new AuthResponseDto<bool>
        {
            Success = false,
            Message = "Not implemented",
            Errors = new List<string> { "Method not implemented" }
        };
    }

    public async Task<AuthResponseDto<bool>> DeleteAccountAsync(string userId, string password, CancellationToken cancellationToken = default)
    {
        // Simplified implementation
        return new AuthResponseDto<bool>
        {
            Success = false,
            Message = "Not implemented",
            Errors = new List<string> { "Method not implemented" }
        };
    }

    public async Task<AuthResponseDto<bool>> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var isAvailable = !await _userService.EmailExistsAsync(email, null, cancellationToken);

            return new AuthResponseDto<bool>
            {
                Success = true,
                Message = isAvailable ? "Email is available" : "Email is already taken",
                Data = isAvailable
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability for: {Email}", email);
            return new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An error occurred while checking email availability",
                Errors = new List<string> { "Email availability check failed. Please try again." }
            };
        }
    }

    // Private helper methods
    private static string GenerateBackupCode()
    {
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();
        return code;
    }
}