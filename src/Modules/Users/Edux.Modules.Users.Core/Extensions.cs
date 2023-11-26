using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Edux.Modules.Users.Api")]
[assembly: InternalsVisibleTo("Edux.Architecture.Tests")]
[assembly: InternalsVisibleTo("Edux.Modules.Users.Tests.Unit")]
namespace Edux.Modules.Users.Core
{
    internal static class Extensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            return services;
        }
    }
}