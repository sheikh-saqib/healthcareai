using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class PrescriptionMedication : BaseEntity
{
    public string PrescriptionMedicationId { get; set; } = Guid.NewGuid().ToString("N");
    public string PrescriptionId { get; set; } = string.Empty;
    public string MedicationName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? BrandName { get; set; }
    public string? DrugCode { get; set; } // NDC code
    public string? RxCUI { get; set; } // RxNorm Concept Unique Identifier
    
    // Dosage information
    public string Dosage { get; set; } = string.Empty;
    public string? Strength { get; set; }
    public string DosageForm { get; set; } = string.Empty; // Tablet, Capsule, Liquid, Injection, etc.
    public string Frequency { get; set; } = string.Empty; // Once daily, Twice daily, etc.
    public string? Duration { get; set; }
    public string? RouteOfAdministration { get; set; } // Oral, Topical, Injection, etc.
    public string? SpecialInstructions { get; set; }
    
    // Quantity and supply
    public int Quantity { get; set; } = 1;
    public string? QuantityUnit { get; set; } = "tablets";
    public int? DaysSupply { get; set; }
    public int? RefillsAllowed { get; set; }
    public int? RefillsUsed { get; set; } = 0;
    
    // Dispensing information
    public bool IsDispensed { get; set; } = false;
    public DateTime? DispensedAt { get; set; }
    public string? DispensedByUserId { get; set; }
    public int? DispensedQuantity { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ManufacturerName { get; set; }
    
    // Financial
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public decimal? InsuranceCovered { get; set; }
    public decimal? PatientCopay { get; set; }
    public decimal? DiscountApplied { get; set; }
    
    // Clinical information
    public string? Indication { get; set; }
    public JsonDocument? Contraindications { get; set; }
    public JsonDocument? SideEffects { get; set; }
    public JsonDocument? DrugInteractions { get; set; }
    public bool IsControlledSubstance { get; set; } = false;
    public string? ControlledSubstanceSchedule { get; set; }
    public bool RequiresMonitoring { get; set; } = false;
    public string? MonitoringInstructions { get; set; }
    
    // Status and tracking
    public string Status { get; set; } = "Pending"; // Pending, Approved, Dispensed, Cancelled
    public string? StatusNotes { get; set; }
    public bool IsSubstitutionAllowed { get; set; } = true;
    public string? SubstitutedWith { get; set; }
    public string? SubstitutionReason { get; set; }
    
    // Navigation properties
    public Prescription Prescription { get; set; } = null!;
    public User? DispensedByUser { get; set; }
} 