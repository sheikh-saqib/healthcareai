using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class LabResult : BaseEntity
{
    public string LabResultId { get; set; } = Guid.NewGuid().ToString("N");
    public string PatientId { get; set; } = string.Empty;
    public string? ConsultationId { get; set; }
    public string? OrderingDoctorId { get; set; }
    public string? LabId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string? TestCode { get; set; }
    public string? TestCategory { get; set; }
    public string? Specimen { get; set; }
    public DateTime? CollectionDate { get; set; }
    public DateTime? TestDate { get; set; }
    public DateTime? ResultDate { get; set; }
    public string? Result { get; set; }
    public string? Unit { get; set; }
    public string? ReferenceRange { get; set; }
    public string? Status { get; set; } = "Pending"; // Pending, Completed, Cancelled
    public string? AbnormalFlag { get; set; } // Normal, High, Low, Critical
    public string? Notes { get; set; }
    public string? TechniciansNotes { get; set; }
    public string? DoctorsNotes { get; set; }
    public bool IsReviewed { get; set; } = false;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public JsonDocument? RawData { get; set; }
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Consultation? Consultation { get; set; }
    public DoctorProfile? OrderingDoctor { get; set; }
    public User? ReviewedByUser { get; set; }
} 