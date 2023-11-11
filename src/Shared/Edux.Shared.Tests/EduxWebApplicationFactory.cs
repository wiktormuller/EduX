using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;
using Testcontainers.RabbitMq;

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
        private readonly RabbitMqContainer _rabbitMqContainer;
        private readonly static string ConnectionStringKey = "sqlserver:connectionString";
        private readonly static string HostNameKey = "rabbitmq:hostnames";
        private readonly static string PortKey = "rabbitmq:port";
        private readonly static string UsernameKey = "rabbitmq:username";
        private readonly static string PasswordKey = "rabbitmq:password";

        public EduxWebApplicationFactory()
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithPassword("Password123")
                .WithCleanUp(true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
                .Build();

            _rabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:latest")
                .WithUsername("guest")
                .WithPassword("guest")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHostBuilder) // Before application gets built
        {
            webHostBuilder.UseEnvironment("test");

            // All components that use the SqlConnectionString will use the version from TestContainer
            webHostBuilder.UseSetting(ConnectionStringKey, _msSqlContainer.GetConnectionString());

            var uri = new Uri(_rabbitMqContainer.GetConnectionString());
            webHostBuilder.UseSetting(HostNameKey, uri.Host);
            webHostBuilder.UseSetting(PortKey, uri.Port.ToString());
            webHostBuilder.UseSetting(UsernameKey, GetUsernameFromUserInfo(uri.UserInfo));
            webHostBuilder.UseSetting(PasswordKey, GetPasswordFromUserInfo(uri.UserInfo));
        }

        public async Task InitializeAsync()
        {
            await _msSqlContainer.StartAsync();

            await _rabbitMqContainer.StartAsync();
        }

        async Task IAsyncLifetime.DisposeAsync()
        {
            // Override when want to dispose something like DbContext, etc.
            await _msSqlContainer.DisposeAsync();
            await base.DisposeAsync();
        }

        private string GetUsernameFromUserInfo(string userInfo)
        {
            return userInfo.Substring(0, userInfo.IndexOf(':'));
        }

        private string GetPasswordFromUserInfo(string userInfo)
        {
            return userInfo.Substring(userInfo.IndexOf(':') + 1);
        }
    }
}
