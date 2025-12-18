using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class PatientDocument : BaseEntity
{
    public string PatientDocumentId { get; set; } = Guid.NewGuid().ToString("N");
    public string PatientId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // ID, Insurance, Medical History, Lab, Image
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public string? Purpose { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? DocumentDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int? DisplayOrder { get; set; }
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public User? VerifiedByUser { get; set; }
} 