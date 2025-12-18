using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class DoctorProfile : BaseEntity
{
    public string DoctorProfileId { get; set; } = Guid.NewGuid().ToString("N");
    public string UserId { get; set; } = string.Empty;
    public string MedicalLicense { get; set; } = string.Empty;
    public string? LicenseExpiryDate { get; set; }
    public string? LicenseIssuingAuthority { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string? SubSpecialization { get; set; }
    public int YearsOfExperience { get; set; } = 0;
    public JsonDocument? Qualifications { get; set; }
    public JsonDocument? Certifications { get; set; }
    public decimal? ConsultationFee { get; set; }
    public JsonDocument? AvailableHours { get; set; }
    public bool IsConsultingOnline { get; set; } = false;
    public string? Biography { get; set; }
    public string? ResearchInterests { get; set; }
    public JsonDocument? Languages { get; set; }
    public string? ProfessionalMemberships { get; set; }
    public string? Awards { get; set; }
    public string? Publications { get; set; }
    public decimal? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public bool IsVerified { get; set; } = false;
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended, Pending
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public ICollection<Patient> PreferredPatients { get; set; } = new List<Patient>();
} 