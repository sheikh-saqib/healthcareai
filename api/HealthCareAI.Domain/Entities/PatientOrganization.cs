using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class PatientOrganization : BaseEntity
{
    public string PatientOrganizationId { get; set; } = Guid.NewGuid().ToString("N");
    public string PatientId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // Primary, Referral, SharedCare, Emergency
    public bool IsActive { get; set; } = true;
    public DateTime? ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }
    public string? ReferredBy { get; set; }
    public string? ReferralReason { get; set; }
    public string? Notes { get; set; }
    public bool CanAccessFullRecord { get; set; } = false;
    public bool CanUpdateRecord { get; set; } = false;
    public string? AccessLevel { get; set; } = "Read"; // Read, Write, Full
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public User? CreatedByUser { get; set; }
} 