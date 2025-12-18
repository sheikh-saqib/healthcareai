namespace HealthCareAI.Application.DTOs;

public class PrescriptionDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ConsultationId { get; set; }
    public string Medications { get; set; } = string.Empty;
    public string? DosageInstructions { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    
    // Navigation properties
    public PatientDto? Patient { get; set; }
    public ConsultationDto? Consultation { get; set; }
}

public class CreatePrescriptionDto
{
    public Guid PatientId { get; set; }
    public Guid ConsultationId { get; set; }
    public string Medications { get; set; } = string.Empty;
    public string? DosageInstructions { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePrescriptionDto
{
    public string? Medications { get; set; }
    public string? DosageInstructions { get; set; }
    public string Status { get; set; } = "pending";
    public string? Notes { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class PrescriptionListDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Medications { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
} 