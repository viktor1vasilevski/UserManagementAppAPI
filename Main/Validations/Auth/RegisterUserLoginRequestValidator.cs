using FluentValidation;
using Main.Requests.Auth;


namespace Main.Validations.Auth;

public class UserRegisterRequestValidator : AbstractValidator<UserRegisterRequest>
{
    public UserRegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is invalid.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\\.[a-zA-Z]{2,})+$")
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Matches(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{6,}$")
            .WithMessage("Password must be at least 6 characters long and include at least one uppercase letter, " +
            "one lowercase letter, one digit, and one special character.");
    }
}

