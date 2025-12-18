using HealthCareAI.Application.DTOs;

namespace HealthCareAI.Application.Interfaces;

public interface IAuthenticationService
{
    // User Registration
    Task<AuthResponseDto<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> VerifyEmailAsync(VerifyEmailRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> ResendEmailVerificationAsync(EmailVerificationRequestDto request, CancellationToken cancellationToken = default);

    // User Login
    Task<AuthResponseDto<LoginResponseDto>> LoginAsync(LoginRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<LoginResponseDto>> CompleteLoginAsync(string twoFactorToken, string code, string? deviceId, bool trustDevice, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> LogoutAsync(string userId, LogoutRequestDto request, CancellationToken cancellationToken = default);

    // Token Management
    Task<AuthResponseDto<TokenResponseDto>> RefreshTokenAsync(TokenRequestDto request, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> RevokeAllTokensAsync(string userId, CancellationToken cancellationToken = default);

    // Password Management
    Task<AuthResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);

    // Two-Factor Authentication
    Task<AuthResponseDto<TwoFactorSetupDto>> SetupTwoFactorAsync(string userId, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> EnableTwoFactorAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> DisableTwoFactorAsync(string userId, string password, CancellationToken cancellationToken = default);

    // User Profile
    Task<AuthResponseDto<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<UserProfileDto>> UpdateProfileAsync(string userId, UserProfileDto profile, CancellationToken cancellationToken = default);

    // Session Management
    Task<AuthResponseDto<List<UserSessionDto>>> GetActiveSessionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> TerminateSessionAsync(string userId, string sessionId, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> TerminateAllSessionsAsync(string userId, string? currentSessionId = null, CancellationToken cancellationToken = default);

    // Account Management
    Task<AuthResponseDto<bool>> DeactivateAccountAsync(string userId, string password, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> DeleteAccountAsync(string userId, string password, CancellationToken cancellationToken = default);
    Task<AuthResponseDto<bool>> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default);
} 