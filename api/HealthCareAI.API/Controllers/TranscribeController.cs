using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranscribeController : ControllerBase
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<TranscribeController> _logger;

    public TranscribeController(
        IOpenAIService openAIService,
        ILogger<TranscribeController> logger)
    {
        _openAIService = openAIService;
        _logger = logger;
    }

    /// <summary>
    /// Transcribes an audio file to text using OpenAI's Whisper model.
    /// </summary>
    /// <param name="audio">The audio file to transcribe</param>
    /// <returns>The transcribed text</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TranscriptionResponseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<TranscriptionResponseDto>> TranscribeAudio(IFormFile audio)
    {
        // Validate file using FluentValidation
        var validator = new HealthCareAI.Application.Validators.FileUploadValidator();
        var validationResult = await validator.ValidateAsync(audio);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            _logger.LogWarning("Audio file validation failed: {Errors}", string.Join(", ", errors));
            
            return BadRequest(new 
            { 
                type = "validation_error",
                title = "File Validation Failed",
                detail = "The uploaded file is invalid.",
                errors = validationResult.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            });
        }

        try
        {
            using var memoryStream = new MemoryStream();
            await audio.CopyToAsync(memoryStream);
            var audioData = memoryStream.ToArray();

            var transcription = await _openAIService.TranscribeAudioAsync(audioData);

            return Ok(new TranscriptionResponseDto { Text = transcription });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcribing audio file");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error transcribing audio file" });
        }
    }
} 