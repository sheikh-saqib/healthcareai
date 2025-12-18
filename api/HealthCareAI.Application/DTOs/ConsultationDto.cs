using System.Text.Json;

namespace HealthCareAI.Application.DTOs;

public class ConsultationDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcription { get; set; }
    public JsonDocument? AiAnalysis { get; set; }
    public string? Symptoms { get; set; }
    public int? Duration { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public PatientDto? Patient { get; set; }
}

public class CreateConsultationDto
{
    public required Guid PatientId { get; set; }
    public string? AudioUrl { get; set; }
    public string? Transcription { get; set; }
    public string? Symptoms { get; set; }
    public int? Duration { get; set; }
}

public class UpdateConsultationDto
{
    public string? AudioUrl { get; set; }
    public string? Transcription { get; set; }
    public JsonDocument? AiAnalysis { get; set; }
    public string? Symptoms { get; set; }
    public int? Duration { get; set; }
    public string Status { get; set; } = "pending";
}

public class ConsultationListDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string? Symptoms { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
} 