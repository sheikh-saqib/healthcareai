using Dapper;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Entities;
using HealthCareAI.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HealthCareAI.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IVerificationTokenRepository _verificationTokenRepository;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public TokenService(
        IConfiguration configuration,
        IVerificationTokenRepository verificationTokenRepository)
    {
        _configuration = configuration;
        _verificationTokenRepository = verificationTokenRepository;
        // Get JWT settings with environment variable fallback
        _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JWT Secret not configured. Set JWT_SECRET_KEY environment variable or JwtSettings:SecretKey in config.");
        _jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "HealthCareAI";
        _jwtAudience = _configuration["JwtSettings:Audience"] ?? "HealthCareAI-Users";
        _accessTokenExpiryMinutes = int.Parse(_configuration["JwtSettings:ExpirationInMinutes"] ?? "60");
        _refreshTokenExpiryDays = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
    }

    // JWT Token Generation
    public async Task<string> GenerateAccessTokenAsync(User user, List<UserRole> roles, List<string> permissions, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new("role", user.Role),
            new("account_status", user.AccountStatus),
            new("email_verified", user.IsEmailVerified.ToString()),
            new("phone_verified", user.IsPhoneVerified.ToString()),
            new("two_factor_enabled", user.IsTwoFactorEnabled.ToString()),
            new("jti", Guid.NewGuid().ToString("N")), // JWT ID for token revocation
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.AccessRole.RoleName));
            claims.Add(new Claim("role_section", $"{role.Section}:{role.AccessRole.RoleName}"));

            if (!string.IsNullOrEmpty(role.SectionId))
            {
                claims.Add(new Claim("role_section_id", $"{role.Section}:{role.SectionId}:{role.AccessRole.RoleName}"));
            }
        }

        // Add permissions
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    // Token Validation
    public async Task<ClaimsPrincipal?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Check if token is revoked
            var jwtToken = validatedToken as JwtSecurityToken;
            var jti = jwtToken?.Claims?.FirstOrDefault(x => x.Type == "jti")?.Value;

            if (!string.IsNullOrEmpty(jti) && await IsTokenRevokedAsync(jti, cancellationToken))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(string refreshToken, string userId, CancellationToken cancellationToken = default)
    {
        // This should be validated against the database (UserSession)
        // Implementation would depend on your session management strategy
        return !string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(userId);
    }

    // Token Information
    public async Task<string?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var principal = await ValidateTokenAsync(token, cancellationToken);
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public async Task<DateTime?> GetTokenExpirationAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            return jsonToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsTokenExpiredAsync(string token, CancellationToken cancellationToken = default)
    {
        var expiration = await GetTokenExpirationAsync(token, cancellationToken);
        return expiration <= DateTime.UtcNow;
    }

    // Token Revocation (In-memory cache or Redis in production)
    private static readonly HashSet<string> _revokedTokens = new();

    public async Task<bool> RevokeTokenAsync(string jwtTokenId, CancellationToken cancellationToken = default)
    {
        _revokedTokens.Add(jwtTokenId);
        return await Task.FromResult(true);
    }

    public async Task<bool> IsTokenRevokedAsync(string jwtTokenId, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_revokedTokens.Contains(jwtTokenId));
    }

    // Security Tokens
    public async Task<string> GenerateEmailVerificationTokenAsync(string userId, string email, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        var token = GenerateSecureToken();
        var verificationToken = new VerificationToken
        {
            VerificationTokenId = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Token = token,
            TokenType = "EmailVerification",
            Email = email,
            ExpiresAt = DateTime.UtcNow.AddHours(24), // 24 hours expiry
            MaxAttempts = 5,
            Purpose = "Email verification for account activation"
        };

        if (transaction != null)
        {
            // Use transaction for atomic operations
            var connection = transaction.Connection ?? throw new InvalidOperationException("Transaction connection is null");

            // Use combined stored procedure to invalidate existing tokens and create new one
            await UpdateTokenWithTransactionAsync(verificationToken, connection, transaction);
        }
        else
        {
            throw new InvalidOperationException("Transaction is null");
        }

        return token;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string userId, string email, CancellationToken cancellationToken = default)
    {
        // Invalidate any existing password reset tokens
        await _verificationTokenRepository.InvalidateTokensByUserIdAsync(userId, "PasswordReset");

        var token = GenerateSecureToken();
        var verificationToken = new VerificationToken
        {
            VerificationTokenId = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Token = token,
            TokenType = "PasswordReset",
            Email = email,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // 1 hour expiry for security
            MaxAttempts = 3,
            Purpose = "Password reset verification"
        };

        await _verificationTokenRepository.CreateAsync(verificationToken);
        return token;
    }

    public async Task<string> GenerateTwoFactorTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Invalidate any existing 2FA tokens
        await _verificationTokenRepository.InvalidateTokensByUserIdAsync(userId, "TwoFactor");

        var token = GenerateSecureToken();
        var verificationToken = new VerificationToken
        {
            VerificationTokenId = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Token = token,
            TokenType = "TwoFactor",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15), // 15 minutes for 2FA
            MaxAttempts = 3,
            Purpose = "Two-factor authentication verification"
        };

        await _verificationTokenRepository.CreateAsync(verificationToken);
        return token;
    }

    public async Task<string> GenerateTrustedDeviceTokenAsync(string userId, string deviceId, CancellationToken cancellationToken = default)
    {
        var token = GenerateSecureToken();
        var verificationToken = new VerificationToken
        {
            VerificationTokenId = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Token = token,
            TokenType = "TrustedDevice",
            ExpiresAt = DateTime.UtcNow.AddDays(30), // 30 days for trusted device
            MaxAttempts = 1,
            Purpose = $"Trusted device token for device: {deviceId}"
        };

        await _verificationTokenRepository.CreateAsync(verificationToken);
        return token;
    }

    // Token Verification
    public async Task<bool> VerifyEmailTokenAsync(string token, string email, CancellationToken cancellationToken = default)
    {
        var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(token, "EmailVerification");

        if (verificationToken == null ||
            verificationToken.Email != email ||
            !await _verificationTokenRepository.CanUseTokenAsync(token, "EmailVerification"))
        {
            if (verificationToken != null)
            {
                await _verificationTokenRepository.IncrementAttemptCountAsync(token);
            }
            return false;
        }

        await _verificationTokenRepository.MarkAsUsedAsync(token);
        return true;
    }

    public async Task<bool> VerifyPasswordResetTokenAsync(string token, string email, CancellationToken cancellationToken = default)
    {
        var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(token, "PasswordReset");

        if (verificationToken == null ||
            verificationToken.Email != email ||
            !await _verificationTokenRepository.CanUseTokenAsync(token, "PasswordReset"))
        {
            if (verificationToken != null)
            {
                await _verificationTokenRepository.IncrementAttemptCountAsync(token);
            }
            return false;
        }

        return true; // Don't mark as used yet - will be marked when password is actually reset
    }

    public async Task<bool> VerifyTwoFactorTokenAsync(string token, string userId, CancellationToken cancellationToken = default)
    {
        var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(token, "TwoFactor");

        if (verificationToken == null ||
            verificationToken.UserId != userId ||
            !await _verificationTokenRepository.CanUseTokenAsync(token, "TwoFactor"))
        {
            if (verificationToken != null)
            {
                await _verificationTokenRepository.IncrementAttemptCountAsync(token);
            }
            return false;
        }

        return true; // Don't mark as used yet - will be marked when 2FA is completed
    }

    public async Task<bool> VerifyTrustedDeviceTokenAsync(string token, string userId, string deviceId, CancellationToken cancellationToken = default)
    {
        var verificationToken = await _verificationTokenRepository.GetByTokenAndTypeAsync(token, "TrustedDevice");

        if (verificationToken == null ||
            verificationToken.UserId != userId ||
            !await _verificationTokenRepository.CanUseTokenAsync(token, "TrustedDevice"))
        {
            return false;
        }

        return true;
    }

    // Token Cleanup
    public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        return await _verificationTokenRepository.CleanupExpiredTokensAsync();
    }

    // Helper Methods
    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    // Transaction-aware helper methods
    private async Task<string> InvalidateTokensByUserIdWithTransactionAsync(string userId, string tokenType, IDbConnection connection, IDbTransaction transaction)
    {
        var sql = @"SELECT * FROM sp_invalidatetokens(@UserId, @TokenType)";

        var parameters = new
        {
            UserId = userId,
            TokenType = tokenType
        };

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters, transaction);
        return result?.message ?? "No result";
    }

    private async Task<string> UpdateTokenWithTransactionAsync(VerificationToken verificationToken, IDbConnection connection, IDbTransaction transaction)
    {
        var sql = @"SELECT * FROM sp_updatetoken(
            @UserId, @TokenType,
            @Id, @VerificationTokenId, @Token, @Email, @Phone,
            @ExpiresAt, @IsUsed, @UsedAt, @IpAddress, @UserAgent,
            @AttemptCount, @LastAttemptAt, @MaxAttempts, @Purpose,
            @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy,
            @IsDeleted, @DeletedAt, @DeletedBy, @RowVersion, @TenantId, @Metadata
        )";

        var parameters = new
        {
            UserId = verificationToken.UserId,
            TokenType = verificationToken.TokenType,
            Id = verificationToken.Id,
            VerificationTokenId = verificationToken.VerificationTokenId,
            Token = verificationToken.Token,
            Email = verificationToken.Email,
            Phone = verificationToken.Phone,
            ExpiresAt = verificationToken.ExpiresAt,
            IsUsed = verificationToken.IsUsed,
            UsedAt = verificationToken.UsedAt,
            IpAddress = verificationToken.IpAddress,
            UserAgent = verificationToken.UserAgent,
            AttemptCount = verificationToken.AttemptCount,
            LastAttemptAt = verificationToken.LastAttemptAt,
            MaxAttempts = verificationToken.MaxAttempts,
            Purpose = verificationToken.Purpose,
            CreatedAt = verificationToken.CreatedAt,
            CreatedBy = verificationToken.CreatedBy,
            UpdatedAt = verificationToken.UpdatedAt,
            UpdatedBy = verificationToken.UpdatedBy,
            IsDeleted = verificationToken.IsDeleted,
            DeletedAt = verificationToken.DeletedAt,
            DeletedBy = verificationToken.DeletedBy,
            RowVersion = verificationToken.RowVersion,
            TenantId = verificationToken.TenantId,
            Metadata = verificationToken.Metadata
        };

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters, transaction);
        return result?.message ?? "No result";
    }

    private async Task CreateVerificationTokenWithTransactionAsync(VerificationToken verificationToken, IDbConnection connection, IDbTransaction transaction)
    {
        var properties = typeof(VerificationToken).GetProperties()
            .Where(p => p.Name != "Id" && p.CanRead)
            .ToList();

        var columns = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
        var parameters = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var sql = $"INSERT INTO \"VerificationTokens\" ({columns}) VALUES ({parameters})";
        await connection.ExecuteAsync(sql, verificationToken, transaction);
    }
}