using FluentValidation;
using Main.Requests.Auth;

namespace Main.Validations.Auth;

public class UserLoginRequestValidator : AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
