using FluentValidation;
using HealthCareAI.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace HealthCareAI.Application.Validators;

public class AnalyzeConsultationRequestDtoValidator : AbstractValidator<AnalyzeConsultationRequestDto>
{
    public AnalyzeConsultationRequestDtoValidator()
    {
        RuleFor(x => x.Transcription)
            .NotEmpty()
            .WithMessage("Transcription is required")
            .MinimumLength(10)
            .WithMessage("Transcription must be at least 10 characters")
            .MaximumLength(50000)
            .WithMessage("Transcription cannot exceed 50,000 characters")
            .Must(ContainMedicalContent)
            .WithMessage("Transcription appears to be invalid or contains no medical content");

        RuleFor(x => x.PatientId)
            .Must(BeValidGuid)
            .When(x => x.PatientId.HasValue)
            .WithMessage("Invalid patient ID format");
    }

    private static bool ContainMedicalContent(string transcription)
    {
        if (string.IsNullOrWhiteSpace(transcription)) return false;
        
        // Basic check for medical-related content
        // Could be enhanced with more sophisticated medical terminology validation
        var medicalKeywords = new[]
        {
            "pain", "symptom", "feel", "hurt", "ache", "doctor", "patient", "medical", 
            "health", "medication", "treatment", "diagnosis", "exam", "checkup",
            "headache", "fever", "cough", "cold", "flu", "sick", "illness", "disease",
            "prescription", "medicine", "drug", "therapy", "surgery", "hospital",
            "clinic", "appointment", "consultation", "visit", "problem", "issue",
            "condition", "history", "allergy", "reaction", "blood", "pressure",
            "heart", "lung", "stomach", "back", "knee", "shoulder", "arm", "leg"
        };

        var lowerTranscription = transcription.ToLowerInvariant();
        return medicalKeywords.Any(keyword => lowerTranscription.Contains(keyword));
    }

    private static bool BeValidGuid(Guid? guid)
    {
        return guid.HasValue && guid.Value != Guid.Empty;
    }
}

public class FileUploadValidator : AbstractValidator<IFormFile>
{
    private readonly string[] _allowedExtensions = { ".wav", ".mp3", ".m4a", ".ogg", ".webm" };
    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25MB

    public FileUploadValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("Audio file is required");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Audio file cannot be empty")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"Audio file size cannot exceed {MaxFileSizeBytes / (1024 * 1024)}MB");

        RuleFor(x => x.FileName)
            .Must(HaveValidExtension)
            .WithMessage($"Audio file must have one of the following extensions: {string.Join(", ", _allowedExtensions)}");

        RuleFor(x => x.ContentType)
            .Must(HaveValidContentType)
            .WithMessage("Invalid audio file type");
    }

    private bool HaveValidExtension(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    private static bool HaveValidContentType(string contentType)
    {
        if (string.IsNullOrEmpty(contentType)) return false;
        
        var validContentTypes = new[]
        {
            "audio/wav", "audio/wave", "audio/x-wav",
            "audio/mpeg", "audio/mp3",
            "audio/mp4", "audio/m4a",
            "audio/ogg", "audio/webm"
        };

        return validContentTypes.Contains(contentType.ToLowerInvariant());
    }
}
