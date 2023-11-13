using Edux.Modules.Users.Application.Contracts.Requests;
using Edux.Modules.Users.Application.Mappers;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using GraphQL;
using Edux.Modules.Users.Application.Graphql.Schemas;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.AspNetCore.Http;
using Edux.Modules.Users.Application.Graphql.Contexts;
using Edux.Modules.Users.Application.Graphql.Messaging;

[assembly: InternalsVisibleTo("Edux.Modules.Users.Api")]
[assembly: InternalsVisibleTo("Edux.Architecture.Tests")]
namespace Edux.Modules.Users.Application
{
    internal static class Extensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IEventMapper, EventMapper>();

            services.AddValidatorsFromAssemblyContaining<GetUserDetailsRequest>();
            services.AddFluentValidationAutoValidation();

            services
                .AddGraphQL(config =>
                {
                    config.AddSchema<UsersSchema>()
                        .AddSystemTextJson()
                        .AddGraphTypes(typeof(UsersSchema).Assembly)
                        .AddUserContextBuilder(httpContext => new UserContext(httpContext));
                })
                .AddWebSockets(config =>
                {
                    config.KeepAliveInterval = TimeSpan.FromSeconds(5);
                });

            services.AddSingleton<UsersMessageService>();

            return services;
        }
    }
}
