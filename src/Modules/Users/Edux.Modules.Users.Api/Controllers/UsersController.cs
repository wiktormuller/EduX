using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Contracts.Requests;
using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Application.Queries;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edux.Modules.Users.Api.Controllers
{
    internal class UsersController : BaseController
    {
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ITokenStorage _tokenStorage;
        private readonly IQueryDispatcher _queryDispatcher;

        public UsersController(ICommandDispatcher commandDispatcher,
            ITokenStorage tokenStorage,
            IQueryDispatcher queryDispatcher)
        {
            _commandDispatcher = commandDispatcher;
            _tokenStorage = tokenStorage;
            _queryDispatcher = queryDispatcher;
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<UserMeResponse>> Get(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(User.Identity?.Name))
            {
                return NotFound();
            }

            var userId = Guid.Parse(User.Identity?.Name); // TODO: It could be taken from IContext
            var query = new GetUserMe(userId);
            var user = await _queryDispatcher.QueryAsync(query, cancellationToken);

            return Ok(user);
        }

        [Authorize(Policy = "is-admin")] // TODO: Implement policies
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<UserResponse>>> Get(GetUsersRequest request, CancellationToken cancellationToken)
        {
            var query = new GetUsers();
            await _queryDispatcher.QueryAsync(query, cancellationToken);
            return Ok();
        }

        [HttpPost("sign-up")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> SignUpAsync(SignUpRequest request, CancellationToken cancellationToken)
        {
            var command = new SignUp(request.Email, request.Username, request.Password, request.Role, request.Claims);

            await _commandDispatcher.SendAsync(command, cancellationToken);
            return CreatedAtAction(nameof(Get), new { command.Email }, null);
        }

        [HttpPost("sign-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JsonWebToken>> SignInAsync(SignInRequest request, CancellationToken cancellationToken)
        {
            var command = new SignIn(request.Email, request.Password);
            await _commandDispatcher.SendAsync(command, cancellationToken);
            var jwt = _tokenStorage.Get();

            return Ok(jwt);
        }

        [HttpPost("refresh-tokens/revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RevokeAccessTokenAsync(RevokeRefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new RevokeRefreshToken(request.RefreshToken);
            await _commandDispatcher.SendAsync(command, cancellationToken);

            return NoContent();
        }

        [HttpPost("refresh-tokens/use")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> UseAccessTokenAsync(UseRefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var command = new UseRefreshToken(request.RefreshToken);
            await _commandDispatcher.SendAsync(command, cancellationToken);

            var jwt = _tokenStorage.Get();

            return Ok(jwt);
        }

        // TODO: Implement access-tokens/revoke based on distributed cache in Pacco
        // TODO: Implement /users
    }
}
