using FluentValidation;
using HealthCareAI.Application.DTOs;

namespace HealthCareAI.Application.Validators;

public class CreateConsultationDtoValidator : AbstractValidator<CreateConsultationDto>
{
    public CreateConsultationDtoValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required");

        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .When(x => x.Duration.HasValue)
            .WithMessage("Duration must be greater than 0");

        RuleFor(x => x.Symptoms)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Symptoms))
            .WithMessage("Symptoms cannot exceed 1000 characters");
    }
}

public class UpdateConsultationDtoValidator : AbstractValidator<UpdateConsultationDto>
{
    public UpdateConsultationDtoValidator()
    {
        RuleFor(x => x.Duration)
            .GreaterThan(0)
            .When(x => x.Duration.HasValue)
            .WithMessage("Duration must be greater than 0");

        RuleFor(x => x.Symptoms)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Symptoms))
            .WithMessage("Symptoms cannot exceed 1000 characters");

        RuleFor(x => x.Status)
            .Must(x => x == "pending" || x == "in_progress" || x == "completed" || x == "cancelled")
            .WithMessage("Invalid status value");
    }
} 