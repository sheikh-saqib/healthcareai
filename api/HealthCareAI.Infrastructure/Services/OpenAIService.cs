using System.Text;
using System.Text.Json;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Application.Models.OpenAI;
using Microsoft.Extensions.Configuration;

namespace HealthCareAI.Infrastructure.Services;

public class OpenAIService : IOpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string? _apiKey;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        // Get API key from environment variable first, then fallback to configuration
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
                 ?? _configuration["OpenAI:ApiKey"]
                 ?? _configuration["OPENAI_API_KEY"];
                 
        // Set timeout for OpenAI requests
        _httpClient.Timeout = TimeSpan.FromMinutes(2);
    }

    public async Task<string> TranscribeAudioAsync(byte[] audioData)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(audioData), "file", "audio.wav");
        content.Add(new StringContent("whisper-1"), "model");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenAI API request failed: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var transcription = JsonSerializer.Deserialize<OpenAITranscriptionResponse>(jsonResponse);
        
        return transcription?.Text ?? string.Empty;
    }

    public async Task<string> AnalyzeConsultationAsync(string transcription, string? patientInfo = null)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        var prompt = $@"
You are a medical AI assistant analyzing a doctor-patient consultation. 

Patient Information:
{patientInfo ?? "Patient information not available - analyze conversation to extract patient details"}

Consultation Transcription:
{transcription}

Please analyze this consultation and provide a JSON response with:
1. symptoms: Array of symptoms mentioned by the patient (even if no patient info provided)
2. patientInfo: Any patient information discovered from the conversation (name, age, medical history, etc.)
3. suggestedActions: Recommended next steps for the doctor
4. prescriptionNeeded: Boolean indicating if prescription is needed
5. medications: If prescription needed, suggest medications with dosage
6. conversationAnalysis: Object with:
   - doctorStatements: Key statements made by the doctor
   - patientStatements: Key statements made by the patient
   - speakerIdentification: Analysis of who said what (based on context clues like medical terminology, questions vs answers)

IMPORTANT: Always extract symptoms and provide analysis even if no patient record is selected. 
Use context clues to identify speakers:
- Medical terminology, diagnosis, and treatment suggestions typically come from doctors
- Symptom descriptions, pain levels, and personal experiences typically come from patients
- Questions about symptoms usually come from doctors
- Answers about how the patient feels usually come from patients

Provide meaningful analysis for any medical conversation, regardless of patient selection.";

        var requestBody = new
        {
            model = "gpt-4",
            messages = new[]
            {
                new { role = "system", content = "You are a medical AI assistant. Always respond with valid JSON." },
                new { role = "user", content = prompt }
            },
            max_tokens = 1000,
            temperature = 0.3
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", jsonContent);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"OpenAI API request failed: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(jsonResponse);
        
        return chatResponse?.Choices?[0]?.Message?.Content ?? "{}";
    }
} 