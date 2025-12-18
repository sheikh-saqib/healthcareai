using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Exceptions;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;
    private readonly ILogger<ConsultationsController> _logger;

    public ConsultationsController(
        IConsultationService consultationService,
        ILogger<ConsultationsController> logger)
    {
        _consultationService = consultationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all consultations with optional filtering by patient ID.
    /// </summary>
    /// <param name="patientId">Optional patient ID to filter consultations</param>
    /// <returns>List of consultations</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConsultationDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<ConsultationDto>>> GetConsultations([FromQuery] string? patientId = null)
    {
        try
        {
            var consultations = await _consultationService.GetAllConsultationsAsync(patientId);
            return Ok(consultations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultations");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error retrieving consultations" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConsultationDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<ConsultationDto>> GetConsultation(string id)
    {
        try
        {
            var consultation = await _consultationService.GetConsultationByIdAsync(id);
            if (consultation == null)
            {
                return NotFound(new { message = $"Consultation with ID {id} not found" });
            }
            return Ok(consultation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving consultation with ID: {ConsultationId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error retrieving consultation" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ConsultationDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ConsultationDto>> CreateConsultation(ConsultationDto createDto)
    {
        try
        {
            var consultation = await _consultationService.CreateConsultationAsync(createDto);
            return CreatedAtAction(nameof(GetConsultation), new { id = consultation.Id }, consultation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consultation");
            return BadRequest(new { message = "Error creating consultation" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ConsultationDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<ConsultationDto>> UpdateConsultation(string id, ConsultationDto updateDto)
    {
        try
        {
            var consultation = await _consultationService.UpdateConsultationAsync(id, updateDto);
            return Ok(consultation);
        }
        catch (ConsultationNotFoundException ex)
        {
            _logger.LogWarning(ex, "Consultation not found");
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating consultation");
            return BadRequest(new { message = "Error updating consultation" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeleteConsultation(string id)
    {
        try
        {
            await _consultationService.DeleteConsultationAsync(id);
            return NoContent();
        }
        catch (ConsultationNotFoundException ex)
        {
            _logger.LogWarning(ex, "Consultation not found");
            return NotFound(new { message = ex.Message });
        }
    }
} 