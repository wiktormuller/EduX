using Edux.Modules.Users.Application.Contracts.Requests;
using Edux.Modules.Users.Application.Graphql.Queries;
using Edux.Modules.Users.Application.Graphql.Subscriptions;
using Edux.Modules.Users.Application.Graphql.Types;
using Edux.Modules.Users.Application.Mappers;
using Edux.Shared.Abstractions.Api.Graphql;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Edux.Modules.Users.Api")]
[assembly: InternalsVisibleTo("Edux.Architecture.Tests")]
namespace Edux.Modules.Users.Application
{
    internal static class Extensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IEventMapper, EventMapper>();

            services.AddValidatorsFromAssemblyContaining<GetUserDetailsRequest>(ServiceLifetime.Singleton);

            services.AddSingleton<IGraphQlModuleQuery, UsersQueries>();
            services.AddSingleton<IGraphQlModuleSubscription, UsersSubscriptions>();

            services.AddSingleton<UserMeType>();
            services.AddSingleton<ReturnedUserMeMessageType>();

            return services;
        }
    }
}
