using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[SwaggerTag("User profile and session management endpoints")]
public class UserController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IAuthenticationService authenticationService,
        ILogger<UserController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    // User Profile
    [HttpGet("profile")]
    [SwaggerOperation(Summary = "Get user profile", Description = "Retrieves current user profile information")]
    [SwaggerResponse(200, "Profile retrieved successfully")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<UserProfileDto>>> GetProfileAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _authenticationService.GetProfileAsync(userId, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new AuthResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpPut("profile")]
    [SwaggerOperation(Summary = "Update user profile", Description = "Updates current user profile information")]
    [SwaggerResponse(200, "Profile updated successfully")]
    [SwaggerResponse(400, "Invalid profile data")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<UserProfileDto>>> UpdateProfileAsync(
        [FromBody] UserProfileDto profile,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponseDto<UserProfileDto>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _authenticationService.UpdateProfileAsync(userId, profile, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new AuthResponseDto<UserProfileDto>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    // Session Management
    [HttpGet("sessions")]
    [SwaggerOperation(Summary = "Get active sessions", Description = "Retrieves all active user sessions")]
    [SwaggerResponse(200, "Sessions retrieved successfully")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<List<UserSessionDto>>>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponseDto<List<UserSessionDto>>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _authenticationService.GetActiveSessionsAsync(userId, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active sessions");
            return StatusCode(500, new AuthResponseDto<List<UserSessionDto>>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpDelete("sessions/{sessionId}")]
    [SwaggerOperation(Summary = "Terminate session", Description = "Terminates a specific user session")]
    [SwaggerResponse(200, "Session terminated successfully")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(404, "Session not found")]
    public async Task<ActionResult<AuthResponseDto<bool>>> TerminateSessionAsync(
        [FromRoute] string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _authenticationService.TerminateSessionAsync(userId, sessionId, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating session");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpDelete("sessions")]
    [SwaggerOperation(Summary = "Terminate all sessions", Description = "Terminates all user sessions except current")]
    [SwaggerResponse(200, "All sessions terminated successfully")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<bool>>> TerminateAllSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var currentSessionId = User.FindFirst("SessionId")?.Value;
            var result = await _authenticationService.TerminateAllSessionsAsync(userId, currentSessionId, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating all sessions");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }
}
