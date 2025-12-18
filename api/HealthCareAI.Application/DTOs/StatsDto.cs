namespace HealthCareAI.Application.DTOs;

public class StatsDto
{
    public int TotalPatients { get; set; }
    public int TodayConsultations { get; set; }
    public int PendingPrescriptions { get; set; }
    public int RecordedConsultations { get; set; }
} 