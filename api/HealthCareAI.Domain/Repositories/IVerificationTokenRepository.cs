using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IVerificationTokenRepository
{
    // Basic CRUD Operations
    Task<VerificationToken?> GetByIdAsync(string tokenId);
    Task<VerificationToken> AddAsync(VerificationToken token);
    Task<VerificationToken> CreateAsync(VerificationToken token);
    Task UpdateAsync(VerificationToken token);
    Task DeleteAsync(VerificationToken token);

    // Core Token Queries
    Task<VerificationToken?> GetByTokenAsync(string token);
    Task<VerificationToken?> GetByTokenAndTypeAsync(string token, string tokenType);
    Task<IEnumerable<VerificationToken>> GetByUserIdAsync(string userId);
    Task<IEnumerable<VerificationToken>> GetByUserIdAndTypeAsync(string userId, string tokenType);
    Task<IEnumerable<VerificationToken>> GetByTypeAsync(string type);
    Task<IEnumerable<VerificationToken>> GetExpiredTokensAsync();

    // Token Operations
    Task<bool> MarkAsUsedAsync(string token);
    Task<bool> InvalidateTokensByUserIdAsync(string userId, string tokenType);

    // Token Validation
    Task<bool> IsTokenValidAsync(string token, string tokenType);
    Task<bool> IsTokenExpiredAsync(string token);
    Task<bool> IsTokenUsedAsync(string token);
    Task<bool> CanUseTokenAsync(string token, string tokenType);

    // Token Queries
    Task<VerificationToken?> GetLatestTokenAsync(string userId, string tokenType);
    Task<int> GetAttemptCountAsync(string token);
    Task<bool> IncrementAttemptCountAsync(string token);
    Task<bool> HasExceededMaxAttemptsAsync(string token);

    // Token Cleanup
    Task<int> CleanupExpiredTokensAsync();
    Task<int> CleanupUsedTokensAsync(int daysToKeep);

    // Utility methods
    Task<int> GetCountAsync();
    Task SaveChangesAsync();
} 