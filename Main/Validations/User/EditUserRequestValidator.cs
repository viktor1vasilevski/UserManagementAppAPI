using FluentValidation;
using Main.Requests.User;

namespace Main.Validations.User;

public class EditUserRequestValidator : AbstractValidator<EditUserRequest>
{
    public EditUserRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is invalid.");
    }
}
