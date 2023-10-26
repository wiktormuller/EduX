using Edux.Modules.Users.Application.Contracts.Requests;
using FluentValidation;

namespace Edux.Modules.Users.Application.Contracts.Validators.Requests
{
    public class GetUsersValidator : AbstractValidator<GetUsersRequest>
    {
        public GetUsersValidator()
        {
        }
    }
}
