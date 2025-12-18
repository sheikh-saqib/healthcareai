using HealthCareAI.Domain.Entities;
using System.Data;
using System.Security.Claims;

namespace HealthCareAI.Application.Interfaces;

public interface ITokenService
{
    // JWT Token Generation
    Task<string> GenerateAccessTokenAsync(User user, List<UserRole> roles, List<string> permissions, CancellationToken cancellationToken = default);
    Task<string> GenerateRefreshTokenAsync(CancellationToken cancellationToken = default);

    // Token Validation
    Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken = default);

    // Token Information
    Task<string?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<DateTime?> GetTokenExpirationAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsTokenExpiredAsync(string token, CancellationToken cancellationToken = default);

    // Token Revocation
    Task<bool> RevokeTokenAsync(string jwtTokenId, CancellationToken cancellationToken = default);
    Task<bool> IsTokenRevokedAsync(string jwtTokenId, CancellationToken cancellationToken = default);

    // Security Tokens
    Task<string> GenerateEmailVerificationTokenAsync(string userId, string email, IDbTransaction? transaction = null, CancellationToken cancellationToken = default);
    Task<string> GeneratePasswordResetTokenAsync(string userId, string email, CancellationToken cancellationToken = default);
    Task<string> GenerateTwoFactorTokenAsync(string userId, CancellationToken cancellationToken = default);
    Task<string> GenerateTrustedDeviceTokenAsync(string userId, string deviceId, CancellationToken cancellationToken = default);

    // Token Verification
    Task<bool> VerifyEmailTokenAsync(string token, string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyPasswordResetTokenAsync(string token, string email, CancellationToken cancellationToken = default);
    Task<bool> VerifyTwoFactorTokenAsync(string token, string userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyTrustedDeviceTokenAsync(string token, string userId, string deviceId, CancellationToken cancellationToken = default);

    // Token Cleanup
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
} 