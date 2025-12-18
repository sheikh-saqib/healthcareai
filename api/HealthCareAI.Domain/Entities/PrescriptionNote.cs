using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class PrescriptionNote : BaseEntity
{
    public string PrescriptionNoteId { get; set; } = Guid.NewGuid().ToString("N");
    public string PrescriptionId { get; set; } = string.Empty;
    public string AuthorUserId { get; set; } = string.Empty;
    public string NoteType { get; set; } = string.Empty; // Clinical, Pharmacy, Patient, System
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime NoteDateTime { get; set; } = DateTime.UtcNow;
    public bool IsPrivate { get; set; } = false;
    public bool IsImportant { get; set; } = false;
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public bool IsPatientVisible { get; set; } = false;
    public bool IsPharmacyVisible { get; set; } = true;
    public int? DisplayOrder { get; set; }
    
    // Navigation properties
    public Prescription Prescription { get; set; } = null!;
    public User Author { get; set; } = null!;
} 