using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class UserPasswordHistory : BaseEntity
{
    public string UserPasswordHistoryId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string HashAlgorithm { get; set; } = "PBKDF2";
    public string? ChangeReason { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    
    // Navigation properties - Commented out for Dapper compatibility
    // When needed, use explicit queries with joins
    // public User User { get; set; } = null!;
    // public User? ChangedByUser { get; set; }
} 