using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _statsService;
    private readonly ILogger<StatsController> _logger;

    public StatsController(
        IStatsService statsService,
        ILogger<StatsController> logger)
    {
        _statsService = statsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets application statistics including patient count, consultations, and prescriptions.
    /// </summary>
    /// <returns>Application statistics</returns>
    [HttpGet]
    [ProducesResponseType(typeof(StatsDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<StatsDto>> GetStats()
    {
        try
        {
            var stats = await _statsService.GetStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving application statistics");
            return StatusCode((int)HttpStatusCode.InternalServerError, 
                new { message = "Error retrieving statistics" });
        }
    }
} 