using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly ILogger<PrescriptionsController> _logger;

    public PrescriptionsController(
        IPrescriptionService prescriptionService,
        ILogger<PrescriptionsController> logger)
    {
        _prescriptionService = prescriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all prescriptions with optional filtering by patient ID and status.
    /// </summary>
    /// <param name="patientId">Optional patient ID to filter prescriptions</param>
    /// <param name="status">Optional status to filter prescriptions</param>
    /// <returns>List of prescriptions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PrescriptionDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<PrescriptionDto>>> GetPrescriptions(
        [FromQuery] string? patientId = null, 
        [FromQuery] string? status = null)
    {
        try
        {
            var prescriptions = await _prescriptionService.GetAllPrescriptionsAsync(patientId, status);
            return Ok(prescriptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescriptions");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error retrieving prescriptions" });
        }
    }

    /// <summary>
    /// Gets a specific prescription by ID.
    /// </summary>
    /// <param name="id">The prescription ID</param>
    /// <returns>The prescription details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PrescriptionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<PrescriptionDto>> GetPrescription(string id)
    {
        try
        {
            var prescription = await _prescriptionService.GetPrescriptionByIdAsync(id);
            if (prescription == null)
            {
                return NotFound(new { message = $"Prescription with ID {id} not found" });
            }
            return Ok(prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prescription with ID: {PrescriptionId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error retrieving prescription" });
        }
    }

    /// <summary>
    /// Creates a new prescription.
    /// </summary>
    /// <param name="createDto">The prescription creation data</param>
    /// <returns>The created prescription</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PrescriptionDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PrescriptionDto>> CreatePrescription(PrescriptionDto createDto)
    {
        try
        {
            var prescription = await _prescriptionService.CreatePrescriptionAsync(createDto);
            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prescription");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error creating prescription" });
        }
    }

    /// <summary>
    /// Updates an existing prescription.
    /// </summary>
    /// <param name="id">The prescription ID</param>
    /// <param name="updateDto">The prescription update data</param>
    /// <returns>The updated prescription</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PrescriptionDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PrescriptionDto>> UpdatePrescription(string id, PrescriptionDto updateDto)
    {
        try
        {
            var prescription = await _prescriptionService.UpdatePrescriptionAsync(id, updateDto);
            return Ok(prescription);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Prescription not found with ID: {PrescriptionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prescription with ID: {PrescriptionId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error updating prescription" });
        }
    }

    /// <summary>
    /// Deletes a prescription.
    /// </summary>
    /// <param name="id">The prescription ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeletePrescription(string id)
    {
        try
        {
            await _prescriptionService.DeletePrescriptionAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Prescription not found with ID: {PrescriptionId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prescription with ID: {PrescriptionId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error deleting prescription" });
        }
    }
} 