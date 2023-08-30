using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Tests
{
    public abstract class WebApiTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        protected readonly HttpClient Client;
        protected readonly WebApplicationFactory<Program> EduxApplicationFactory;

        protected WebApiTestBase(WebApplicationFactory<Program> factory, string environment = "test")
        {
            EduxApplicationFactory = factory.WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.UseEnvironment(environment);
                webHostBuilder.ConfigureServices(ConfigureServices);
            });
            Client = EduxApplicationFactory.CreateClient();
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Template method when we want to override some service registration
        }

        public virtual void Dispose()
        {
            // Override when want to dispose something like DbContext, etc.
        }
    }
}
