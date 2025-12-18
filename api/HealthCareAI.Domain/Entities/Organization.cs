using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class Organization : BaseEntity
{
    public string OrganizationId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Hospital, Clinic, Practice, Pharmacy
    public string LicenseNumber { get; set; } = string.Empty;
    public string TaxIdentificationNumber { get; set; } = string.Empty;
    public string PrimaryContactEmail { get; set; } = string.Empty;
    public string PrimaryContactPhone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    
    // Address as structured data
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Business details
    public string SubscriptionTier { get; set; } = "Basic";
    public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;
    public DateTime? SubscriptionEndDate { get; set; }
    public int MaxUsers { get; set; } = 10;
    public int MaxPatients { get; set; } = 1000;
    
    // Status and metadata
    public string Status { get; set; } = "Active"; // Active, Suspended, Pending, Cancelled
    public bool IsActive { get; set; } = true;
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public string Language { get; set; } = "en-US";
    
    // Compliance and settings
    public JsonDocument? Settings { get; set; }
    public JsonDocument? ComplianceInfo { get; set; }
    public DateTime? LastAuditDate { get; set; }
    public string? AuditStatus { get; set; }
    
    // Navigation properties
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<OrganizationSetting> OrganizationSettings { get; set; } = new List<OrganizationSetting>();
    public ICollection<OrganizationAuthPolicy> AuthPolicies { get; set; } = new List<OrganizationAuthPolicy>();
    public ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
    public ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
} 