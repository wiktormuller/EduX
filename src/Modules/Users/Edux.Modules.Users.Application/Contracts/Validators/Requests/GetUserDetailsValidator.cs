using Edux.Modules.Users.Application.Contracts.Requests;
using FluentValidation;

namespace Edux.Modules.Users.Application.Contracts.Validators.Requests
{
    public class GetUserDetailsValidator : AbstractValidator<GetUserDetailsRequest>
    {
        public GetUserDetailsValidator()
        {
            RuleFor(request => request.UserId)
                .NotEmpty();
        }
    }
}
