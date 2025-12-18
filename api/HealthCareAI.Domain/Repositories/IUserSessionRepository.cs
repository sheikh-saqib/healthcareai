using HealthCareAI.Domain.Entities;

namespace HealthCareAI.Domain.Repositories;

public interface IUserSessionRepository
{
    // Session Management
    Task<UserSession?> GetByIdAsync(string sessionId);
    Task<UserSession?> GetBySessionTokenAsync(string sessionToken);
    Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<UserSession>> GetByUserIdAsync(string userId);
    Task<IEnumerable<UserSession>> GetExpiredSessionsAsync();

    // Session Operations
    Task<UserSession> AddAsync(UserSession session);
    Task<UserSession> CreateAsync(UserSession session);
    Task UpdateAsync(UserSession session);
    Task DeleteAsync(UserSession session);
    Task DeactivateSessionAsync(string sessionToken);
    Task DeactivateAllUserSessionsAsync(string userId);
    Task DeactivateExpiredSessionsAsync();
    Task<bool> DeactivateAsync(string sessionId);
    Task<bool> DeactivateAllByUserIdAsync(string userId, string? excludeSessionId = null);

    // Utility methods
    Task<int> GetCountAsync();
    Task SaveChangesAsync();
} 