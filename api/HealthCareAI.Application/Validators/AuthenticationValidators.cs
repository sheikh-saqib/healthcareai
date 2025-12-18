using FluentValidation;
using HealthCareAI.Application.DTOs;
using System.Text.RegularExpressions;

namespace HealthCareAI.Application.Validators;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters")
            .Must(BeStrongPassword)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Confirm password is required")
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required")
            .MaximumLength(50)
            .WithMessage("First name cannot exceed 50 characters")
            .Must(BeValidName)
            .WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required")
            .MaximumLength(50)
            .WithMessage("Last name cannot exceed 50 characters")
            .Must(BeValidName)
            .WithMessage("Last name contains invalid characters");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.OrganizationId)
            .Must(BeValidGuid)
            .When(x => !string.IsNullOrEmpty(x.OrganizationId))
            .WithMessage("Invalid organization ID format");
    }

    private static bool BeStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
    }

    private static bool BeValidName(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        
        return Regex.IsMatch(name, @"^[a-zA-Z\s\-'\.]+$");
    }

    private static bool BeValidGuid(string? guid)
    {
        if (string.IsNullOrEmpty(guid)) return true;
        
        return Guid.TryParse(guid, out _);
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters");

        RuleFor(x => x.TwoFactorCode)
            .Matches(@"^\d{6}$")
            .When(x => !string.IsNullOrEmpty(x.TwoFactorCode))
            .WithMessage("Two-factor code must be 6 digits");

        RuleFor(x => x.DeviceName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.DeviceName))
            .WithMessage("Device name cannot exceed 100 characters");
    }
}

public class ChangePasswordRequestDtoValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestDtoValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters")
            .Must(BeStrongPassword)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Confirm new password is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");
    }

    private static bool BeStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
    }
}

public class TwoFactorRequestDtoValidator : AbstractValidator<TwoFactorRequestDto>
{
    public TwoFactorRequestDtoValidator()
    {
        RuleFor(x => x.TwoFactorToken)
            .NotEmpty()
            .WithMessage("Two-factor token is required");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .Matches(@"^\d{6}$")
            .WithMessage("Code must be 6 digits");
    }
}

public class ForgotPasswordRequestDtoValidator : AbstractValidator<ForgotPasswordRequestDto>
{
    public ForgotPasswordRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");
    }
}

public class ResetPasswordRequestDtoValidator : AbstractValidator<ResetPasswordRequestDto>
{
    public ResetPasswordRequestDtoValidator()
    {
        RuleFor(x => x.ResetToken)
            .NotEmpty()
            .WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters")
            .Must(BeStrongPassword)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty()
            .WithMessage("Confirm new password is required")
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords do not match");
    }

    private static bool BeStrongPassword(string password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        
        return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$");
    }
}
