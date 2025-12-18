using System.Net;
using System.Text.Json;
using FluentValidation;

namespace HealthCareAI.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}", 
                correlationId, context.Request.Path, context.Request.Method);
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString("N")[..8];
        
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        return correlationId;
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ValidationException validationEx => new ErrorResponse
            {
                Type = "validation_error",
                Title = "Validation Failed",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = "One or more validation errors occurred.",
                CorrelationId = correlationId,
                Errors = validationEx.Errors?.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                Type = "unauthorized",
                Title = "Unauthorized",
                Status = (int)HttpStatusCode.Unauthorized,
                Detail = "You are not authorized to access this resource.",
                CorrelationId = correlationId
            },
            KeyNotFoundException => new ErrorResponse
            {
                Type = "not_found",
                Title = "Resource Not Found",
                Status = (int)HttpStatusCode.NotFound,
                Detail = "The requested resource was not found.",
                CorrelationId = correlationId
            },
            ArgumentException argEx => new ErrorResponse
            {
                Type = "bad_request",
                Title = "Bad Request",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = _environment.IsDevelopment() ? argEx.Message : "The request was invalid.",
                CorrelationId = correlationId
            },
            InvalidOperationException invalidOpEx => new ErrorResponse
            {
                Type = "invalid_operation",
                Title = "Invalid Operation",
                Status = (int)HttpStatusCode.BadRequest,
                Detail = _environment.IsDevelopment() ? invalidOpEx.Message : "The operation is not valid for the current state.",
                CorrelationId = correlationId
            },
            HttpRequestException httpEx => new ErrorResponse
            {
                Type = "external_service_error",
                Title = "External Service Error",
                Status = (int)HttpStatusCode.BadGateway,
                Detail = "An external service is currently unavailable. Please try again later.",
                CorrelationId = correlationId
            },
            TimeoutException => new ErrorResponse
            {
                Type = "timeout",
                Title = "Request Timeout",
                Status = (int)HttpStatusCode.RequestTimeout,
                Detail = "The request took too long to process. Please try again.",
                CorrelationId = correlationId
            },
            _ => new ErrorResponse
            {
                Type = "internal_error",
                Title = "Internal Server Error",
                Status = (int)HttpStatusCode.InternalServerError,
                Detail = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An internal error occurred. Please contact support if the problem persists.",
                CorrelationId = correlationId
            }
        };

        context.Response.StatusCode = response.Status;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        var result = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string[]>? Errors { get; set; }
} 