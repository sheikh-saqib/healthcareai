using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class VerificationToken : BaseEntity
{
    public string VerificationTokenId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty; // EmailVerification, PhoneVerification, PasswordReset
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public int AttemptCount { get; set; } = 0;
    public DateTime? LastAttemptAt { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public string? Purpose { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
} 