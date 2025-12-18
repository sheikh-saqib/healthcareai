using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class Patient : BaseEntity
{
    public string PatientId { get; set; } = Guid.NewGuid().ToString("N");
    public string OrganizationId { get; set; } = string.Empty;
    public string PatientNumber { get; set; } = string.Empty; // Auto-generated per org
    public string MedicalRecordNumber { get; set; } = string.Empty;
    
    // Personal information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public string? PreferredLanguage { get; set; } = "en-US";
    public string? MaritalStatus { get; set; }
    public string? Occupation { get; set; }
    
    // Contact information
    public string PrimaryPhone { get; set; } = string.Empty;
    public string? SecondaryPhone { get; set; }
    public string? Email { get; set; }
    public string? AlternateEmail { get; set; }
    
    // Address information
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Emergency contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }
    public string? EmergencyContactAddress { get; set; }
    
    // Medical information
    public string? BloodType { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? HeightUnit { get; set; } = "cm";
    public string? WeightUnit { get; set; } = "kg";
    public JsonDocument? Allergies { get; set; }
    public JsonDocument? ChronicConditions { get; set; }
    public JsonDocument? CurrentMedications { get; set; }
    public JsonDocument? Immunizations { get; set; }
    public string? MedicalHistory { get; set; }
    public string? FamilyHistory { get; set; }
    public string? SocialHistory { get; set; }
    public string? SurgicalHistory { get; set; }
    
    // Insurance information
    public JsonDocument? InsuranceInfo { get; set; }
    public string? PrimaryInsuranceProvider { get; set; }
    public string? PolicyNumber { get; set; }
    public string? GroupNumber { get; set; }
    public string? SecondaryInsuranceProvider { get; set; }
    public string? SecondaryPolicyNumber { get; set; }
    
    // Account information
    public string Status { get; set; } = "Active"; // Active, Inactive, Deceased, Transferred
    public bool IsActive { get; set; } = true;
    public string? PreferredDoctorId { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime? NextAppointmentDate { get; set; }
    public string? Notes { get; set; }
    public JsonDocument? Preferences { get; set; }
    public JsonDocument? Tags { get; set; }
    
    // Privacy and consent
    public bool ConsentToTreatment { get; set; } = false;
    public bool ConsentToDataSharing { get; set; } = false;
    public bool ConsentToMarketing { get; set; } = false;
    public DateTime? ConsentDate { get; set; }
    public string? ConsentVersion { get; set; }
    
    // Registration information
    public DateTime? RegistrationDate { get; set; }
    public string? RegistrationSource { get; set; }
    public string? ReferredBy { get; set; }
    public string? ReferralNotes { get; set; }
    
    // Navigation properties - only include entities that exist in DbContext
    // public Organization Organization { get; set; } = null!;
    // public DoctorProfile? PreferredDoctor { get; set; }
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    // public ICollection<PatientOrganization> PatientOrganizations { get; set; } = new List<PatientOrganization>();
    // public ICollection<VitalSign> VitalSigns { get; set; } = new List<VitalSign>();
    // public ICollection<LabResult> LabResults { get; set; } = new List<LabResult>();
    // public ICollection<PatientDocument> PatientDocuments { get; set; } = new List<PatientDocument>();
} 