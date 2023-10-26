using Edux.Modules.Users.Application.Contracts.Requests;
using FluentValidation;

namespace Edux.Modules.Users.Application.Contracts.Validators.Requests
{
    public class SignInValidator : AbstractValidator<SignInRequest>
    {
        public SignInValidator()
        {
            RuleFor(request => request.Email)
                .EmailAddress()
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(200);

            RuleFor(request => request.Password)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(200);
        }
    }
}
