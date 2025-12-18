using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyzeConsultationController : ControllerBase
    {
        private readonly IOpenAIService _openAIService;
        private readonly IPatientRepository _patientRepository;
        private readonly IActivityService _activityService;
        private readonly ILogger<AnalyzeConsultationController> _logger;

        public AnalyzeConsultationController(
            IOpenAIService openAIService,
            IPatientRepository patientRepository,
            IActivityService activityService,
            ILogger<AnalyzeConsultationController> logger)
        {
            _openAIService = openAIService;
            _patientRepository = patientRepository;
            _activityService = activityService;
            _logger = logger;
        }

    /// <summary>
    /// Analyzes a consultation transcription using AI to extract symptoms, patient info, and recommendations.
    /// </summary>
    /// <param name="request">The consultation analysis request containing transcription and optional patient ID</param>
    /// <returns>The AI analysis results</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AnalysisResponseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<AnalysisResponseDto>> AnalyzeConsultation([FromBody] AnalyzeConsultationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Transcription))
        {
            _logger.LogWarning("Empty transcription provided for analysis");
            return BadRequest(new { message = "Transcription is required" });
        }

        try
        {
            string? patientInfo = null;
            
            if (request.PatientId.HasValue)
            {
                var patientIdString = request.PatientId.Value.ToString("N");
                var patient = await _patientRepository.GetByIdAsync(patientIdString);
                if (patient != null)
                {
                    patientInfo = $"Name: {patient.FullName}, Age: {DateTime.Now.Year - patient.DateOfBirth.Year}, Medical History: {patient.MedicalHistory}";
                }
            }

            var analysisResult = await _openAIService.AnalyzeConsultationAsync(request.Transcription, patientInfo);
            
            // Try to parse the JSON response
            try
            {
                var analysis = JsonDocument.Parse(analysisResult);
                var response = new AnalysisResponseDto { Analysis = analysis };
                
                // Broadcast real-time update
                await _activityService.BroadcastConsultationAnalyzedAsync(new
                {
                    PatientId = request.PatientId,
                    Transcription = request.Transcription.Length > 100 ? request.Transcription.Substring(0, 100) + "..." : request.Transcription,
                    Analysis = analysis
                });
                
                return Ok(response);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse AI analysis response as JSON");
                
                // If JSON parsing fails, return a default structure
                var defaultAnalysis = new ConsultationAnalysisDto
                {
                    Symptoms = Array.Empty<string>(),
                    PatientInfo = new { },
                    SuggestedActions = Array.Empty<string>(),
                    PrescriptionNeeded = false,
                    Medications = Array.Empty<string>(),
                    ConversationAnalysis = new ConversationAnalysisDto
                    {
                        DoctorStatements = Array.Empty<string>(),
                        PatientStatements = Array.Empty<string>(),
                        SpeakerIdentification = "Unable to parse analysis"
                    }
                };

                var defaultJson = JsonDocument.Parse(JsonSerializer.Serialize(defaultAnalysis));
                return Ok(new AnalysisResponseDto { Analysis = defaultJson });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing consultation transcription");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error analyzing consultation" });
        }
    }
} 