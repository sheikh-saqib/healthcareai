using System.Text.Json;

namespace HealthCareAI.Application.DTOs;

public class AnalyzeConsultationRequestDto
{
    public string Transcription { get; set; } = string.Empty;
    public Guid? PatientId { get; set; }
}

public class AnalysisResponseDto
{
    public JsonDocument Analysis { get; set; } = JsonDocument.Parse("{}");
}

public class ConsultationAnalysisDto
{
    public string[] Symptoms { get; set; } = Array.Empty<string>();
    public object? PatientInfo { get; set; }
    public string[] SuggestedActions { get; set; } = Array.Empty<string>();
    public bool PrescriptionNeeded { get; set; }
    public string[] Medications { get; set; } = Array.Empty<string>();
    public ConversationAnalysisDto? ConversationAnalysis { get; set; }
}

public class ConversationAnalysisDto
{
    public string[] DoctorStatements { get; set; } = Array.Empty<string>();
    public string[] PatientStatements { get; set; } = Array.Empty<string>();
    public string SpeakerIdentification { get; set; } = string.Empty;
} 