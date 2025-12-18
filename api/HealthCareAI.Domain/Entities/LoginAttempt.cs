using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class LoginAttempt : BaseEntity
{
    public string LoginAttemptId { get; set; } = Guid.NewGuid().ToString("N");
    public string? UserId { get; set; } // Null for failed attempts with invalid email
    public string EmailAttempted { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; } = false;
    public string? FailureReason { get; set; }
    public string? OrganizationId { get; set; }
    public string? DeviceId { get; set; }
    public string? Location { get; set; }
    public string? SessionId { get; set; }
    public JsonDocument? AdditionalData { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    public Organization? Organization { get; set; }
} 