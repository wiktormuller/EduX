using Edux.Modules.Users.Application.Contracts.Requests;
using FluentValidation;

namespace Edux.Modules.Users.Application.Contracts.Validators.Requests
{
    public class UseRefreshTokenValidator : AbstractValidator<UseRefreshTokenRequest>
    {
        public UseRefreshTokenValidator()
        {
            RuleFor(request => request.RefreshToken)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(200);
        }
    }
}
