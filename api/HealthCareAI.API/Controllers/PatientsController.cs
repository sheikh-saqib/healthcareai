using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(
        IPatientService patientService,
        ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all patients with optional search functionality.
    /// </summary>
    /// <param name="search">Optional search term to filter patients</param>
    /// <returns>List of patients</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PatientDto>), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients([FromQuery] string? search = null)
    {
        var patients = await _patientService.GetAllPatientsAsync(search);
        return Ok(patients);

    }

    /// <summary>
    /// Gets a specific patient by ID.
    /// </summary>
    /// <param name="id">The patient ID</param>
    /// <returns>The patient details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PatientDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<PatientDto>> GetPatient(string id)
    {
        var patient = await _patientService.GetPatientByIdAsync(id);
        if (patient == null)
        {
            return NotFound(new { message = $"Patient with ID {id} not found" });
        }
        return Ok(patient);
    }

    /// <summary>
    /// Creates a new patient.
    /// </summary>
    /// <param name="createDto">The patient creation data</param>
    /// <returns>The created patient</returns>
    [HttpPost]
    [ProducesResponseType(typeof(PatientDto), (int)HttpStatusCode.Created)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PatientDto>> CreatePatient(PatientDto createDto)
    {
        var patient = await _patientService.CreatePatientAsync(createDto);
        return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient);
    }

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    /// <param name="id">The patient ID</param>
    /// <param name="updateDto">The patient update data</param>
    /// <returns>The updated patient</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PatientDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PatientDto>> UpdatePatient(string id, PatientDto updateDto)
    {
        try
        {
            var patient = await _patientService.UpdatePatientAsync(id, updateDto);
            return Ok(patient);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Patient not found with ID: {PatientId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient with ID: {PatientId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "Error updating patient" });
        }
    }

    /// <summary>
    /// Deletes a patient.
    /// </summary>
    /// <param name="id">The patient ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> DeletePatient(string id)
    {
        try
        {
            await _patientService.DeletePatientAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Patient not found with ID: {PatientId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient with ID: {PatientId}", id);
            return StatusCode((int)HttpStatusCode.InternalServerError,
                new { message = "Error deleting patient" });
        }
    }
}