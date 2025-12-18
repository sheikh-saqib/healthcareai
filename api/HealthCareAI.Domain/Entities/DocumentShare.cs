using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class DocumentShare : BaseEntity
{
    public string DocumentShareId { get; set; } = Guid.NewGuid().ToString("N");
    public string DocumentId { get; set; } = string.Empty;
    public string SharedWithUserId { get; set; } = string.Empty;
    public string SharedByUserId { get; set; } = string.Empty;
    public string ShareType { get; set; } = string.Empty; // View, Edit, Download
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ShareReason { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public int AccessCount { get; set; } = 0;
    public bool RequiresNotification { get; set; } = true;
    public bool IsNotified { get; set; } = false;
    public DateTime? NotifiedAt { get; set; }
    
    // Navigation properties
    public Document Document { get; set; } = null!;
    public User SharedWithUser { get; set; } = null!;
    public User SharedByUser { get; set; } = null!;
} 