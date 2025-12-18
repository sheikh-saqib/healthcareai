using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class Prescription : BaseEntity
{
    public string PrescriptionId { get; set; } = Guid.NewGuid().ToString("N");
    public string PatientId { get; set; } = string.Empty;
    public string ConsultationId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string PrescriptionNumber { get; set; } = string.Empty;
    
    // Prescription details
    public DateTime PrescribedDate { get; set; } = DateTime.UtcNow;
    public string? GeneralInstructions { get; set; }
    public string? Duration { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Dispensed, Cancelled, Expired
    public DateTime? ValidUntil { get; set; }
    public int? RefillsAllowed { get; set; }
    public int? RefillsUsed { get; set; } = 0;
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent
    
    // Review and approval
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public string? ReviewStatus { get; set; } = "Pending"; // Pending, Approved, Rejected
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByUserId { get; set; }
    
    // Dispensing information
    public DateTime? DispensedAt { get; set; }
    public string? DispensedByUserId { get; set; }
    public string? PharmacyId { get; set; }
    public string? PharmacyName { get; set; }
    public string? PharmacistName { get; set; }
    public string? PharmacistLicense { get; set; }
    
    // Financial
    public decimal? TotalCost { get; set; }
    public decimal? InsuranceCovered { get; set; }
    public decimal? PatientCopay { get; set; }
    public decimal? DiscountApplied { get; set; }
    public string? PaymentStatus { get; set; } = "Pending";
    public string? PaymentMethod { get; set; }
    
    // Quality and safety
    public JsonDocument? AllergiesChecked { get; set; }
    public JsonDocument? DrugInteractions { get; set; }
    public JsonDocument? Contraindications { get; set; }
    public bool IsHighRisk { get; set; } = false;
    public string? RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
    public JsonDocument? SafetyAlerts { get; set; }
    
    // Additional info
    public JsonDocument? CustomFields { get; set; }
    public bool IsElectronic { get; set; } = true;
    public string? PrintedBy { get; set; }
    public DateTime? PrintedAt { get; set; }
    public int? PrintCount { get; set; } = 0;
    
    // Patient communication
    public bool PatientNotified { get; set; } = false;
    public DateTime? PatientNotifiedAt { get; set; }
    public string? PatientNotificationMethod { get; set; } // SMS, Email, Phone, Portal
    
    // Navigation properties - only include entities that exist in DbContext
    public Patient Patient { get; set; } = null!;
    public Consultation Consultation { get; set; } = null!;
    // public DoctorProfile Doctor { get; set; } = null!;
    // public Organization Organization { get; set; } = null!;
    // public User? ReviewedByUser { get; set; }
    // public User? ApprovedByUser { get; set; }
    // public User? DispensedByUser { get; set; }
    // public ICollection<PrescriptionMedication> Medications { get; set; } = new List<PrescriptionMedication>();
    // public ICollection<PrescriptionNote> Notes { get; set; } = new List<PrescriptionNote>();
    // public ICollection<PrescriptionRefill> Refills { get; set; } = new List<PrescriptionRefill>();
} 