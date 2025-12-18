using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class ConsultationParticipant : BaseEntity
{
    public string ConsultationParticipantId { get; set; } = Guid.NewGuid().ToString("N");
    public string ConsultationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Primary Doctor, Consultant, Nurse, Observer, Student
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public bool CanViewRecords { get; set; } = true;
    public bool CanEditRecords { get; set; } = false;
    public bool CanPrescribe { get; set; } = false;
    public string? InvitedByUserId { get; set; }
    public DateTime? InvitedAt { get; set; }
    public string? ParticipationStatus { get; set; } = "Invited"; // Invited, Joined, Left, Declined
    
    // Navigation properties - Commented out for Dapper compatibility
    // public Consultation Consultation { get; set; } = null!;
    // public User User { get; set; } = null!;
    // public User? InvitedByUser { get; set; }
} 