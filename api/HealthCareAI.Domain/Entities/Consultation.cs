using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class Consultation : BaseEntity
{
    public string ConsultationId { get; set; } = Guid.NewGuid().ToString("N");
    public string PatientId { get; set; } = string.Empty;
    public string DoctorId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public string ConsultationNumber { get; set; } = string.Empty;
    
    // Appointment details
    public string ConsultationType { get; set; } = string.Empty; // In-Person, Video, Phone, Emergency
    public DateTime? ScheduledDateTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
    public int? DurationMinutes { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, In-Progress, Completed, Cancelled, No-Show
    public string? CancellationReason { get; set; }
    public string Priority { get; set; } = "Normal"; // Low, Normal, High, Urgent, Emergency
    
    // Medical content
    public string? ChiefComplaint { get; set; }
    public string? PresentIllness { get; set; }
    public string? Symptoms { get; set; }
    public string? PhysicalExamination { get; set; }
    public string? Assessment { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? FollowUpInstructions { get; set; }
    public JsonDocument? DiagnosisCodes { get; set; } // ICD-10 codes
    public JsonDocument? ProcedureCodes { get; set; } // CPT codes
    
    // AI and digital content
    public string? AudioFileUrl { get; set; }
    public string? VideoFileUrl { get; set; }
    public string? Transcription { get; set; }
    public JsonDocument? AiAnalysis { get; set; }
    public JsonDocument? AiSuggestions { get; set; }
    public JsonDocument? AiConfidenceScores { get; set; }
    public bool IsAiAssisted { get; set; } = false;
    
    // Financial
    public decimal? ConsultationFee { get; set; }
    public decimal? AmountPaid { get; set; }
    public string? PaymentStatus { get; set; } = "Pending"; // Pending, Paid, Partial, Cancelled
    public string? PaymentMethod { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime? BillingDate { get; set; }
    
    // Quality and compliance
    public int? PatientSatisfactionRating { get; set; }
    public string? PatientFeedback { get; set; }
    public JsonDocument? QualityMetrics { get; set; }
    public string? ReviewStatus { get; set; } = "Pending"; // Pending, Reviewed, Approved
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    
    // Additional information
    public string? Location { get; set; }
    public string? Room { get; set; }
    public JsonDocument? CustomFields { get; set; }
    public bool IsTelemedicine { get; set; } = false;
    public string? TelemedicineUrl { get; set; }
    public bool RequiresFollowUp { get; set; } = false;
    public DateTime? FollowUpDate { get; set; }
    
    // Navigation properties - only include entities that exist in DbContext
    public Patient Patient { get; set; } = null!;
    // public DoctorProfile Doctor { get; set; } = null!;
    // public Organization Organization { get; set; } = null!;
    // public Department? Department { get; set; }
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    // public ICollection<ConsultationParticipant> Participants { get; set; } = new List<ConsultationParticipant>();
    // public ICollection<ConsultationDocument> Documents { get; set; } = new List<ConsultationDocument>();
    // public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
    // public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    // public ICollection<ConsultationNote> Notes { get; set; } = new List<ConsultationNote>();
} 