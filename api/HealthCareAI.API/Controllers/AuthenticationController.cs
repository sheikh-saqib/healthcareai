using HealthCareAI.Application.DTOs;
using HealthCareAI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace HealthCareAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Authentication endpoints")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ITokenService tokenService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _tokenService = tokenService;
        _logger = logger;
    }

    // User Registration
    [HttpPost("register")]  
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Register a new user", Description = "Creates a new user account with email verification required")]
    [SwaggerResponse(200, "Registration successful", typeof(AuthResponseDto<RegisterResponseDto>))]
    [SwaggerResponse(400, "Invalid request data")]
    [SwaggerResponse(409, "Email already exists")]
    public async Task<ActionResult<AuthResponseDto<RegisterResponseDto>>> RegisterAsync(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto<RegisterResponseDto>
            {
                Success = false,
                Message = "Invalid request data",
                Errors = [.. ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)]
            });
        }

        var result = await _authenticationService.RegisterAsync(request, cancellationToken);

        return result.Success ? Ok(result) : BadRequest(result);
    }

    // Email Verification
    [HttpPost("verify-email")]
    [SwaggerOperation(Summary = "Verify email address", Description = "Verifies user email address using verification token")]
    [SwaggerResponse(200, "Email verified successfully")]
    [SwaggerResponse(400, "Invalid or expired token")]
    public async Task<ActionResult<AuthResponseDto<bool>>> VerifyEmailAsync(
        [FromBody] VerifyEmailRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authenticationService.VerifyEmailAsync(request, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpPost("resend-verification")]
    [SwaggerOperation(Summary = "Resend email verification", Description = "Resends email verification token")]
    [SwaggerResponse(200, "Verification email sent")]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<ActionResult<AuthResponseDto<bool>>> ResendEmailVerificationAsync(
        [FromBody] EmailVerificationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authenticationService.ResendEmailVerificationAsync(request, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending verification email");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    // User Login
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "User login", Description = "Authenticates user and returns JWT tokens")]
    [SwaggerResponse(200, "Login successful", typeof(AuthResponseDto<LoginResponseDto>))]
    [SwaggerResponse(400, "Invalid credentials")]
    [SwaggerResponse(401, "Authentication failed")]
    public async Task<ActionResult<AuthResponseDto<LoginResponseDto>>> LoginAsync(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponseDto<LoginResponseDto>
            {
                Success = false,
                Message = "Invalid request data",
                Errors = [.. ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)]
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

        var result = await _authenticationService.LoginAsync(request, ipAddress, userAgent, cancellationToken);

        return result.Success ? Ok(result) : Unauthorized(result);
    }

    // Token Management
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Refresh access token", Description = "Refreshes JWT access token using refresh token")]
    [SwaggerResponse(200, "Token refreshed successfully")]
    [SwaggerResponse(400, "Invalid refresh token")]
    public async Task<ActionResult<AuthResponseDto<TokenResponseDto>>> RefreshTokenAsync(
        [FromBody] TokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userAgent = HttpContext.Request.Headers.UserAgent.ToString();

            var result = await _authenticationService.RefreshTokenAsync(request, ipAddress, userAgent, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new AuthResponseDto<TokenResponseDto>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    // Logout
    [HttpPost("logout")]
    [Authorize]
    [SwaggerOperation(Summary = "User logout", Description = "Logs out user and invalidates tokens")]
    [SwaggerResponse(200, "Logout successful")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<bool>>> LogoutAsync(
        [FromBody] LogoutRequestDto request,
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

            var result = await _authenticationService.LogoutAsync(userId, request, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    // Password Management
    [HttpPost("change-password")]
    [Authorize]
    [SwaggerOperation(Summary = "Change password", Description = "Changes user password")]
    [SwaggerResponse(200, "Password changed successfully")]
    [SwaggerResponse(400, "Invalid current password")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<bool>>> ChangePasswordAsync(
        [FromBody] ChangePasswordRequestDto request,
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

            var result = await _authenticationService.ChangePasswordAsync(userId, request, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Forgot password", Description = "Initiates password reset process")]
    [SwaggerResponse(200, "Password reset email sent")]
    [SwaggerResponse(400, "Invalid email")]
    public async Task<ActionResult<AuthResponseDto<bool>>> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authenticationService.ForgotPasswordAsync(request, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Reset password", Description = "Resets user password using reset token")]
    [SwaggerResponse(200, "Password reset successfully")]
    [SwaggerResponse(400, "Invalid reset token")]
    public async Task<ActionResult<AuthResponseDto<bool>>> ResetPasswordAsync(
        [FromBody] ResetPasswordRequestDto request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authenticationService.ResetPasswordAsync(request, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    // Utility Endpoints
    [HttpGet("check-email")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Check email availability", Description = "Checks if email address is available for registration")]
    [SwaggerResponse(200, "Email availability checked")]
    [SwaggerResponse(400, "Invalid email")]
    public async Task<ActionResult<AuthResponseDto<bool>>> CheckEmailAvailabilityAsync(
        [FromQuery] string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new AuthResponseDto<bool>
                {
                    Success = false,
                    Message = "Email address is required"
                });
            }

            var result = await _authenticationService.CheckEmailAvailabilityAsync(email, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    [SwaggerOperation(Summary = "Revoke refresh token", Description = "Revokes a specific refresh token")]
    [SwaggerResponse(200, "Token revoked successfully")]
    [SwaggerResponse(400, "Invalid token")]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<ActionResult<AuthResponseDto<bool>>> RevokeTokenAsync(
        [FromBody] string refreshToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _authenticationService.RevokeTokenAsync(refreshToken, cancellationToken);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return StatusCode(500, new AuthResponseDto<bool>
            {
                Success = false,
                Message = "An internal server error occurred"
            });
        }
    }
}