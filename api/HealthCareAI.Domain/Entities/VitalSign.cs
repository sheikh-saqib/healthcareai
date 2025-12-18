using HealthCareAI.Domain.Common;

namespace HealthCareAI.Domain.Entities;

public class VitalSign : BaseEntity
{
    public string VitalSignId { get; set; } = Guid.NewGuid().ToString("N");
    public string PatientId { get; set; } = string.Empty;
    public string? ConsultationId { get; set; }
    public string RecordedByUserId { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string? Location { get; set; }
    public string? DeviceUsed { get; set; }
    
    // Vital measurements
    public decimal? Temperature { get; set; }
    public string? TemperatureUnit { get; set; } = "F";
    public int? BloodPressureSystolic { get; set; }
    public int? BloodPressureDiastolic { get; set; }
    public int? HeartRate { get; set; }
    public int? RespiratoryRate { get; set; }
    public decimal? OxygenSaturation { get; set; }
    public decimal? Height { get; set; }
    public decimal? Weight { get; set; }
    public string? HeightUnit { get; set; } = "cm";
    public string? WeightUnit { get; set; } = "kg";
    public decimal? BMI { get; set; }
    public decimal? BloodGlucose { get; set; }
    public string? BloodGlucoseUnit { get; set; } = "mg/dL";
    public decimal? PainLevel { get; set; } // 0-10 scale
    
    // Additional measurements
    public decimal? WaistCircumference { get; set; }
    public decimal? HipCircumference { get; set; }
    public decimal? HeadCircumference { get; set; }
    public string? PulseRhythm { get; set; } // Regular, Irregular
    public string? BloodPressurePosition { get; set; } // Sitting, Standing, Lying
    public string? Notes { get; set; }
    
    // Calculated values
    public decimal? WaistHipRatio { get; set; }
    public decimal? BodySurfaceArea { get; set; }
    public string? BMICategory { get; set; } // Underweight, Normal, Overweight, Obese
    
    // Quality indicators
    public bool IsNormalRange { get; set; } = true;
    public string? AbnormalFlags { get; set; } // High, Low, Critical
    public bool RequiresAttention { get; set; } = false;
    public string? AlertLevel { get; set; } // Normal, Warning, Critical
    
    // Navigation properties
    public Patient Patient { get; set; } = null!;
    public Consultation? Consultation { get; set; }
    public User RecordedByUser { get; set; } = null!;
} 