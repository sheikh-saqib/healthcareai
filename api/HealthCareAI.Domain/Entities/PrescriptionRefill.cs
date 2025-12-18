using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class PrescriptionRefill : BaseEntity
{
    public string PrescriptionRefillId { get; set; } = Guid.NewGuid().ToString("N");
    public string PrescriptionId { get; set; } = string.Empty;
    public string RequestedByUserId { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Denied, Dispensed
    public string? RequestReason { get; set; }
    public string? Notes { get; set; }
    public int RefillNumber { get; set; } = 1;
    public int QuantityRequested { get; set; }
    public int? QuantityApproved { get; set; }
    public int? QuantityDispensed { get; set; }
    
    // Approval information
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }
    public string? ApprovalNotes { get; set; }
    public DateTime? DeniedAt { get; set; }
    public string? DeniedByUserId { get; set; }
    public string? DenialReason { get; set; }
    
    // Dispensing information
    public DateTime? DispensedAt { get; set; }
    public string? DispensedByUserId { get; set; }
    public string? PharmacyId { get; set; }
    public string? PharmacyName { get; set; }
    public string? PharmacistName { get; set; }
    public decimal? Cost { get; set; }
    public string? PaymentMethod { get; set; }
    
    // Navigation properties
    public Prescription Prescription { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public User? ApprovedByUser { get; set; }
    public User? DeniedByUser { get; set; }
    public User? DispensedByUser { get; set; }
} 