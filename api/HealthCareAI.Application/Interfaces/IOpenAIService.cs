namespace HealthCareAI.Application.Interfaces;
 
public interface IOpenAIService
{
    Task<string> TranscribeAudioAsync(byte[] audioData);
    Task<string> AnalyzeConsultationAsync(string transcription, string? patientInfo = null);
} 