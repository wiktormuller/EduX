using Edux.Modules.Users.Application.Contracts.Requests;
using FluentValidation;

namespace Edux.Modules.Users.Application.Contracts.Validators.Requests
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            RuleFor(request => request.Role)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(20);
        }
    }
}
