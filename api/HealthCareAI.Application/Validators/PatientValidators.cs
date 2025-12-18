using FluentValidation;
using HealthCareAI.Application.DTOs;
using System.Text.RegularExpressions;

namespace HealthCareAI.Application.Validators;

public class CreatePatientDtoValidator : AbstractValidator<CreatePatientDto>
{
    public CreatePatientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Patient name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters")
            .Must(BeValidName)
            .WithMessage("Name contains invalid characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Age)
            .GreaterThan(0)
            .WithMessage("Age must be greater than 0")
            .LessThan(150)
            .WithMessage("Age must be less than 150");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("Gender is required")
            .Must(BeValidGender)
            .WithMessage("Gender must be Male, Female, or Other");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.MedicalHistory))
            .WithMessage("Medical history cannot exceed 2000 characters");
    }

    private static bool BeValidName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        
        return Regex.IsMatch(name, @"^[a-zA-Z\s\-'\.]+$");
    }

    private static bool BeValidGender(string gender)
    {
        var validGenders = new[] { "Male", "Female", "Other", "male", "female", "other", "M", "F", "O" };
        return validGenders.Contains(gender);
    }
}

public class UpdatePatientDtoValidator : AbstractValidator<UpdatePatientDto>
{
    public UpdatePatientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Patient name is required")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters")
            .Must(BeValidName)
            .WithMessage("Name contains invalid characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Age)
            .GreaterThan(0)
            .WithMessage("Age must be greater than 0")
            .LessThan(150)
            .WithMessage("Age must be less than 150");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .WithMessage("Gender is required")
            .Must(BeValidGender)
            .WithMessage("Gender must be Male, Female, or Other");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Address))
            .WithMessage("Address cannot exceed 500 characters");

        RuleFor(x => x.MedicalHistory)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.MedicalHistory))
            .WithMessage("Medical history cannot exceed 2000 characters");
    }

    private static bool BeValidName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        
        return Regex.IsMatch(name, @"^[a-zA-Z\s\-'\.]+$");
    }

    private static bool BeValidGender(string gender)
    {
        var validGenders = new[] { "Male", "Female", "Other", "male", "female", "other", "M", "F", "O" };
        return validGenders.Contains(gender);
    }
}
