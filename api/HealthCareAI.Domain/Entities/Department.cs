using HealthCareAI.Domain.Common;
using System.Text.Json;

namespace HealthCareAI.Domain.Entities;

public class Department : BaseEntity
{
    public string DepartmentId { get; set; } = Guid.NewGuid().ToString("N");
    public string OrganizationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // CARD, EMER, GP, etc.
    public string? Description { get; set; }
    public string? HeadOfDepartmentUserId { get; set; }
    public string? Location { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int MaxCapacity { get; set; } = 0;
    public decimal? BudgetAllocated { get; set; }
    public bool IsActive { get; set; } = true;
    public JsonDocument? Settings { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public User? HeadOfDepartment { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public ICollection<DoctorSchedule> DoctorSchedules { get; set; } = new List<DoctorSchedule>();
} 