using System.Diagnostics;
using System.Text;

namespace HealthCareAI.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Response.Headers["X-Correlation-ID"].FirstOrDefault() 
                           ?? Guid.NewGuid().ToString("N")[..8];
        
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        var stopwatch = Stopwatch.StartNew();
        var requestTime = DateTime.UtcNow;
        
        // Log request
        await LogRequestAsync(context, correlationId, requestTime);
        
        // Capture original response body stream
        var originalBodyStream = context.Response.Body;
        
        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            
            await _next(context);
            
            stopwatch.Stop();
            
            // Log response
            await LogResponseAsync(context, correlationId, stopwatch.ElapsedMilliseconds, responseBody);
            
            // Copy the response back to the original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task LogRequestAsync(HttpContext context, string correlationId, DateTime requestTime)
    {
        var request = context.Request;
        
        var requestLog = new
        {
            CorrelationId = correlationId,
            Timestamp = requestTime,
            Method = request.Method,
            Path = request.Path.Value,
            QueryString = request.QueryString.Value,
            Headers = GetSafeHeaders(request.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value))),
            UserAgent = request.Headers["User-Agent"].FirstOrDefault(),
            IpAddress = GetClientIpAddress(context),
            UserId = context.User?.Identity?.Name,
            ContentLength = request.ContentLength,
            ContentType = request.ContentType
        };

        _logger.LogInformation("HTTP Request: {Method} {Path} - CorrelationId: {CorrelationId}", 
            request.Method, request.Path, correlationId);
        
        _logger.LogDebug("HTTP Request Details: {@RequestLog}", requestLog);

        // Log request body for specific endpoints (excluding file uploads)
        if (ShouldLogRequestBody(request))
        {
            var requestBody = await ReadRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(requestBody))
            {
                _logger.LogDebug("Request Body: {RequestBody} - CorrelationId: {CorrelationId}", 
                    requestBody, correlationId);
            }
        }
    }

    private async Task LogResponseAsync(HttpContext context, string correlationId, long elapsedMs, MemoryStream responseBody)
    {
        var response = context.Response;
        
        var responseLog = new
        {
            CorrelationId = correlationId,
            StatusCode = response.StatusCode,
            ElapsedMs = elapsedMs,
            ContentLength = response.ContentLength ?? responseBody.Length,
            ContentType = response.ContentType,
            Headers = GetSafeHeaders(response.Headers.Select(h => new KeyValuePair<string, IEnumerable<string>>(h.Key, h.Value)))
        };

        var logLevel = GetLogLevel(response.StatusCode);
        
        _logger.Log(logLevel, "HTTP Response: {StatusCode} - {ElapsedMs}ms - CorrelationId: {CorrelationId}", 
            response.StatusCode, elapsedMs, correlationId);
        
        _logger.LogDebug("HTTP Response Details: {@ResponseLog}", responseLog);

        // Log response body for errors
        if (response.StatusCode >= 400 && ShouldLogResponseBody(response))
        {
            var responseBodyText = await ReadResponseBodyAsync(responseBody);
            if (!string.IsNullOrEmpty(responseBodyText))
            {
                _logger.LogWarning("Error Response Body: {ResponseBody} - CorrelationId: {CorrelationId}", 
                    responseBodyText, correlationId);
            }
        }

        // Log performance warnings
        if (elapsedMs > 5000) // 5 seconds
        {
            _logger.LogWarning("Slow Response: {Method} {Path} took {ElapsedMs}ms - CorrelationId: {CorrelationId}", 
                context.Request.Method, context.Request.Path, elapsedMs, correlationId);
        }
    }

    private static LogLevel GetLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => LogLevel.Error,
            >= 400 => LogLevel.Warning,
            _ => LogLevel.Information
        };
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        // Don't log file uploads or large payloads
        if (request.ContentType?.Contains("multipart/form-data") == true) return false;
        if (request.ContentType?.Contains("application/octet-stream") == true) return false;
        if (request.ContentLength > 10 * 1024) return false; // 10KB limit
        
        // Log for JSON/XML content
        return request.ContentType?.Contains("application/json") == true ||
               request.ContentType?.Contains("application/xml") == true ||
               request.ContentType?.Contains("text/") == true;
    }

    private static bool ShouldLogResponseBody(HttpResponse response)
    {
        // Log error responses and small JSON responses
        if (response.StatusCode >= 400) return true;
        if (response.ContentLength > 10 * 1024) return false; // 10KB limit
        
        return response.ContentType?.Contains("application/json") == true;
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        try
        {
            request.EnableBuffering();
            var body = request.Body;
            body.Seek(0, SeekOrigin.Begin);
            
            using var reader = new StreamReader(body, Encoding.UTF8, leaveOpen: true);
            var bodyText = await reader.ReadToEndAsync();
            body.Seek(0, SeekOrigin.Begin);
            
            return bodyText;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static async Task<string> ReadResponseBodyAsync(MemoryStream responseBody)
    {
        try
        {
            responseBody.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true);
            var bodyText = await reader.ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);
            
            return bodyText;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static Dictionary<string, string> GetSafeHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        var sensitiveHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Authorization", "Cookie", "Set-Cookie", "X-API-Key", "X-Auth-Token"
        };

        return headers
            .Where(h => !sensitiveHeaders.Contains(h.Key))
            .ToDictionary(h => h.Key, h => string.Join(", ", h.Value), StringComparer.OrdinalIgnoreCase);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Check for forwarded headers (when behind a proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
