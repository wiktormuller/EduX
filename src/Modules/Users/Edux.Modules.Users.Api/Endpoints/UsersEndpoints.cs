using Edux.Modules.Users.Api.Helpers;
using Edux.Modules.Users.Application.Commands;
using Edux.Modules.Users.Application.Contracts.Requests;
using Edux.Modules.Users.Application.Contracts.Responses;
using Edux.Modules.Users.Application.Queries;
using Edux.Shared.Abstractions.Auth;
using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Edux.Modules.Users.Api.Endpoints
{
    internal static class UsersEndpoints
    {
        public static void AddUsersEndpoints(this IEndpointRouteBuilder builder)
        {
            var usersGroup = builder
                .MapGroup(UsersModule.BasePath + "/users")
                .WithName("Users")
                .WithTags("Users")
                .AddEndpointFilterFactory(ValidationHelper.ValidationFilterFactory);

            usersGroup
                .MapGet("me",
                async (HttpContext httpContext, IQueryDispatcher queryDispatcher, CancellationToken cancellationToken) =>
                {
                    if (string.IsNullOrWhiteSpace(httpContext.User?.Identity?.Name))
                    {
                        return Results.NotFound();
                    }

                    var userId = Guid.Parse(httpContext.User.Identity.Name);
                    var query = new GetUserMe(userId);
                    var user = await queryDispatcher.QueryAsync(query, cancellationToken);

                    return Results.Ok(user);
                })
                .WithName("me")
                .RequireRateLimiting("jwt")
                .Produces<UserMeResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();

            usersGroup
                .MapGet("",
                async (IQueryDispatcher queryDispatcher, CancellationToken cancellationToken) =>
                {
                    var query = new GetUsers();
                    var users = await queryDispatcher.QueryAsync(query, cancellationToken);
                    return Results.Ok(users);
                })
                .Produces<IEnumerable<UserResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .RequireAuthorization("is-admin");

            usersGroup
                .MapPost("sign-up",
                async ([FromBody] [Validate] SignUpRequest request, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new SignUp(request.Email, request.Username, request.Password, request.Role, request.Claims);

                    await commandDispatcher.SendAsync(command, cancellationToken);
                    return Results.CreatedAtRoute("me", new { command.Email }, null);
                })
                .Produces(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);

            usersGroup
                .MapPost("sign-in",
                async ([FromBody] [Validate] SignInRequest request, ICommandDispatcher commandDispatcher, ITokenStorage tokenStorage, CancellationToken cancellationToken) =>
                {
                    var command = new SignIn(request.Email, request.Password);
                    await commandDispatcher.SendAsync(command, cancellationToken);
                    var jwt = tokenStorage.Get();

                    return Results.Ok(jwt);
                })
                .Produces<JsonWebToken>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            usersGroup
                .MapPost("refresh-tokens/revoke",
                async ([FromBody] [Validate] RevokeRefreshTokenRequest request, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new RevokeRefreshToken(request.RefreshToken);
                    await commandDispatcher.SendAsync(command, cancellationToken);

                    return Results.NoContent();
                })
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status400BadRequest);

            usersGroup
                .MapPost("refresh-tokens/use",
                async ([FromBody] [Validate] UseRefreshTokenRequest request, ICommandDispatcher commandDispatcher, ITokenStorage tokenStorage, CancellationToken cancellationToken) =>
                {
                    var command = new UseRefreshToken(request.RefreshToken);
                    await commandDispatcher.SendAsync(command, cancellationToken);

                    var jwt = tokenStorage.Get();

                    return Results.Ok(jwt);
                })
                .Produces<JsonWebToken>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);

            usersGroup
                .MapPut("{id:guid}",
                async ([FromRoute] Guid id, [FromBody] [Validate] UpdateUserRequest request, ICommandDispatcher commandDispatcher, CancellationToken cancellationToken) =>
                {
                    var command = new UpdateUser(id, request.Role, request.IsActive, request.Claims);
                    await commandDispatcher.SendAsync(command, cancellationToken);

                    return Results.NoContent();
                })
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status204NoContent)
                .RequireAuthorization("is-admin");

            // TODO: Implement attatching user logo image to it's account
            // TODO: Implement resetting password by user
            // TODO: Implement activating account by user
        }
    }
}
