using FluentValidation;
using Identity.Domain;
using System.Text.RegularExpressions;

namespace Identity.Application.Commands.RegisterUser
{
    /// <summary>
    /// FluentValidation validator for RegisterUserCommand.
    /// Same pattern as CreateShortenedUrlValidator in the Shortening module.
    /// 
    /// IMPLEMENTATION NOTES:
    /// 
    /// 1. This runs BEFORE the handler via the ValidationBehavior pipeline.
    ///    If validation fails, a ValidationException is thrown and the handler
    ///    never executes. The ValidationExceptionHandler in the API project
    ///    catches it and returns a 400 Bad Request with the error messages.
    ///    
    /// 2. Validation here is for INPUT FORMAT — not business rules.
    ///    Business rules (like "email already exists") belong in the handler.
    ///    
    /// 3. Password rules — balance security with user experience:
    ///    - Minimum 8 characters (NIST recommendation)
    ///    - You can add: must contain uppercase, lowercase, digit, special char
    ///    - Maximum length (e.g., 128) to prevent DoS via extremely long passwords
    /// </summary>
    public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(User.MinUsernameLength).WithMessage($"Username must be at least {User.MinUsernameLength} characters.")
                .MaximumLength(User.MaxUsernameLength).WithMessage($"Username must not exceed {User.MaxUsernameLength} characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(User.MinPasswordLength).WithMessage($"Password must be at least {User.MinPasswordLength} characters.")
                .MaximumLength(User.MaxPasswordLength).WithMessage($"Password must not exceed {User.MaxPasswordLength} characters.")
                .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches(@"[0-9]").WithMessage("Password must contain at least one digit.");
        }
    }
}
