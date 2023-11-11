using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Abstractions.Modules
{
    public interface IModule
    {
        string Name { get; }
        string Path { get; }
        IEnumerable<string> Policies => new List<string>();
        void Register(IServiceCollection services);
        void Use(IApplicationBuilder app);
    }
}
