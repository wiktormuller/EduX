using Edux.Modules.Users.Application.Contracts.Requests;
using FluentValidation;

namespace Edux.Modules.Users.Application.Contracts.Validators.Requests
{
    public class SignUpValidator : AbstractValidator<SignUpRequest>
    {
        public SignUpValidator()
        {
            RuleFor(request => request.Username)
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(200);

            RuleFor(request => request.Email)
                .EmailAddress()
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(200);

            RuleFor(request => request.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(200);

            RuleFor(request => request.Role)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(20);
        }
    }
}
