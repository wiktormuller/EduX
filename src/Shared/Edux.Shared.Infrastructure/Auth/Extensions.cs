using Edux.Shared.Abstractions.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Auth
{
    internal static class Extensions
    {
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddSingleton<IPasswordService, PasswordService>();
            services.AddSingleton<IJwtProvider, JwtProvider>();
            services.AddSingleton<ITokenStorage, HttpContextTokenStorage>();

            return services;
        }
    }
}
