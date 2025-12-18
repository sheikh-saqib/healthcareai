using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string AuditLogId { get; set; } = Guid.NewGuid().ToString("N");
    public string? UserId { get; set; }
    public string? OrganizationId { get; set; }
    public string? SessionId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Login, Logout
    public string? EntityType { get; set; }
    public string? OldValues { get; set; } // Stored as jsonb, but using string for Dapper
    public string? NewValues { get; set; } // Stored as jsonb, but using string for Dapper  
    public string? ChangedFields { get; set; } // Stored as jsonb, but using string for Dapper
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
    public string? CorrelationId { get; set; }
    
    // Risk and compliance
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
    public string? ComplianceFlags { get; set; }
    public bool IsSecurityRelevant { get; set; } = false;
    public bool RequiresReview { get; set; } = false;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    
    // Additional context
    public string? Module { get; set; }
    public string? Feature { get; set; }
    public string? Method { get; set; }
    public string? Endpoint { get; set; }
    public int? ResponseCode { get; set; }
    public long? ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AdditionalData { get; set; } // Stored as jsonb, but using string for Dapper
    
    // Patient context (for HIPAA compliance)
    public string? PatientId { get; set; }
    public string? PatientContext { get; set; }
    public string? AccessReason { get; set; }
    public bool IsPatientDataAccess { get; set; } = false;
    
    // Navigation properties - Commented out for Dapper compatibility
    // public User? User { get; set; }
    // public Organization? Organization { get; set; }
    // public Patient? Patient { get; set; }
    // public User? ReviewedByUser { get; set; }
} 