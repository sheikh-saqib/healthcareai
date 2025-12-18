using HealthCareAI.API.Extensions;
using HealthCareAI.API.Middleware;
using HealthCareAI.API.Hubs;
using HealthCareAI.API.Services;
using HealthCareAI.Application.Interfaces;
using HealthCareAI.Infrastructure;
using HealthCareAI.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql;
using Serilog;
using Serilog.Events;

// Ensure wwwroot directory exists before creating builder
var contentRoot = Directory.GetCurrentDirectory();
var wwwrootPath = Path.Combine(contentRoot, "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
}

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = contentRoot,
    WebRootPath = wwwrootPath
});

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/healthcare-ai-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 100 * 1024 * 1024, // 100MB
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting HealthCareAI API");

// Configure Dapper type handlers
Dapper.SqlMapper.AddTypeHandler(new HealthCareAI.Infrastructure.Data.JsonDocumentTypeHandler());

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configure model validation
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true; // We'll handle validation in our middleware
});

// Get configuration from environment variables (production) or config files (development)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string not found. Set DATABASE_CONNECTION_STRING environment variable or DefaultConnection in config.");

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? jwtSettings["SecretKey"] 
    ?? throw new InvalidOperationException("JWT SecretKey not found. Set JWT_SECRET_KEY environment variable or JwtSettings:SecretKey in config.");

// Validate secret key strength in production
if (builder.Environment.IsProduction() && secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT secret key must be at least 32 characters in production.");
}
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HealthCare AI API", Version = "v1" });
    c.EnableAnnotations();
    // Add JWT Authentication support in Swagger
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and your token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Configure CORS
var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',')
    ?? builder.Configuration["Security:AllowedOrigins"]?.Split(',')
    ?? new[] { "http://localhost:3000", "http://localhost:3001" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", corsBuilder =>
    {
        corsBuilder.WithOrigins(allowedOrigins)
                   .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                   .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                   .AllowCredentials()
                   .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
    
    options.AddPolicy("Development", corsBuilder =>
    {
        corsBuilder.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:5000")
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
    });
});

// Configure SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});

// Configure Application Services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register ActivityService in API project
builder.Services.AddScoped<IActivityService, ActivityService>();

// Add Rate Limiting - Temporarily disabled for testing
// Note: Rate limiting can be implemented with a third-party package or custom middleware
// var enableRateLimiting = false; // Disabled until proper package is available

// Configure Caching
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? builder.Configuration["Caching:Redis:ConnectionString"];

if (!string.IsNullOrEmpty(redisConnectionString) && redisConnectionString != "localhost:6379" || builder.Environment.IsProduction())
{
    try
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = builder.Configuration["Caching:Redis:InstanceName"] ?? "HealthCareAI";
        });
        
        Log.Information("Redis distributed caching configured: {ConnectionString}", redisConnectionString?.Split('@').LastOrDefault());
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to configure Redis caching, falling back to in-memory cache");
        builder.Services.AddMemoryCache();
    }
}
else
{
    // Use in-memory cache for development
    builder.Services.AddMemoryCache();
    Log.Information("Using in-memory caching for development");
}

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
});

// Add Response Caching
builder.Services.AddResponseCaching(options =>
{
    options.MaximumBodySize = 1024 * 1024; // 1MB
    options.UseCaseSensitivePaths = false;
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck("Database Connection", () =>
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
        }
        catch
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
        }
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HealthCare AI v1");
        c.RoutePrefix = "swagger"; // Serve the Swagger UI at /swagger
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Request/Response logging (before exception handling)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

// Global error handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Add response compression
app.UseResponseCompression();

// Add response caching
app.UseResponseCaching();

app.UseHttpsRedirection();

// Use appropriate CORS policy based on environment
app.UseCors(app.Environment.IsDevelopment() ? "Development" : "Production");

// Enable rate limiting if configured - Temporarily disabled
// if (enableRateLimiting)
// {
//     app.UseRateLimiter();
// }

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Map SignalR Hub
app.MapHub<ActivityHub>("/activityHub");

// Serve static files (for SPA support)
app.UseDefaultFiles();
app.UseStaticFiles();

// Add fallback for SPA
app.MapFallbackToFile("index.html");

// Verify existing database connection and structure
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var databaseInitializer = services.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync();
        
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database verification completed successfully");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while verifying the database.");
    }
}

    Log.Information("HealthCareAI API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "HealthCareAI API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for testing
public partial class Program { }
