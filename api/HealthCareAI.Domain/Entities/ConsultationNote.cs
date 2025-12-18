using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class ConsultationNote : BaseEntity
{
    public string ConsultationNoteId { get; set; } = Guid.NewGuid().ToString("N");
    public string ConsultationId { get; set; } = string.Empty;
    public string AuthorUserId { get; set; } = string.Empty;
    public string NoteType { get; set; } = string.Empty; // Progress, Assessment, Plan, Observation
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime NoteDateTime { get; set; } = DateTime.UtcNow;
    public bool IsPrivate { get; set; } = false;
    public bool IsImportant { get; set; } = false;
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public int? DisplayOrder { get; set; }
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }
    public string? EditedBy { get; set; }
    public string? EditReason { get; set; }
    
    // Navigation properties
    public Consultation Consultation { get; set; } = null!;
    public User Author { get; set; } = null!;
    public User? EditedByUser { get; set; }
} 