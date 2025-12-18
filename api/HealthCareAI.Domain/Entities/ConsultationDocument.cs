using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class ConsultationDocument : BaseEntity
{
    public string ConsultationDocumentId { get; set; } = Guid.NewGuid().ToString("N");
    public string ConsultationId { get; set; } = string.Empty;
    public string DocumentId { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // Lab Result, X-Ray, Prescription, Photo, Report
    public string? Category { get; set; }
    public string? Notes { get; set; }
    public string? Purpose { get; set; }
    public bool IsRequired { get; set; } = false;
    public bool IsReviewed { get; set; } = false;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public string? ReviewNotes { get; set; }
    public int? DisplayOrder { get; set; }
    
    // Navigation properties
    public Consultation Consultation { get; set; } = null!;
    public Document Document { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
} 