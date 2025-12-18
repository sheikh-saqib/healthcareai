using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class DoctorSchedule : BaseEntity
{
    public string DoctorScheduleId { get; set; } = Guid.NewGuid().ToString("N");
    public string DoctorProfileId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public int DayOfWeek { get; set; } // 0=Sunday, 1=Monday, etc.
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ScheduleType { get; set; } = "Regular"; // Regular, Holiday, Exception, OnCall
    public DateTime? SpecificDate { get; set; } // For specific date schedules
    public int? SlotDurationMinutes { get; set; } = 30;
    public int? MaxPatientsPerSlot { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? Location { get; set; }
    public string? Room { get; set; }
    public JsonDocument? Settings { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public DoctorProfile DoctorProfile { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public Department? Department { get; set; }
} 