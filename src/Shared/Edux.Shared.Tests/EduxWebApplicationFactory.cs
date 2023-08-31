using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using Edux.Shared.Tests.Helpers;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;

namespace Edux.Shared.Tests
{
    public class EduxWebApplicationFactory<TProgram> : WebApplicationFactory<Program>, IAsyncLifetime 
        where TProgram : class
    {
        public readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly MsSqlContainer _msSqlContainer;
        private readonly static string ConnectionStringKey = "sqlserver:connectionString";

        public EduxWebApplicationFactory()
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Password123")
                .WithCleanUp(true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.UseEnvironment("test");
            webHostBuilder.ConfigureTestServices(ConfigureServices);

            webHostBuilder.UseSetting(ConnectionStringKey, _msSqlContainer.GetConnectionString());
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Template method when we want to override some service registration
        }

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            // Override when want to dispose something like DbContext, etc.
            //await _databaseContainer.DisposeAsync();
        }
    }
}
