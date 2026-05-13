using FluentValidation;

namespace Identity.Application.Queries.LoginUser
{
    public class LoginUserValidator : AbstractValidator<LoginUserQuery>
    {
        public LoginUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
