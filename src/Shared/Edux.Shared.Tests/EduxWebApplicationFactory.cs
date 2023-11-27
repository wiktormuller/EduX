using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Testcontainers.MsSql;
using DotNet.Testcontainers.Builders;
using Testcontainers.RabbitMq;
using Testcontainers.MongoDb;
using Testcontainers.Redis;

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
        private readonly MongoDbContainer _mongoDbContainer;
        private readonly RedisContainer _redisContainer;

        private readonly static string SqlServerConnectionStringKey = "sqlserver:connectionString";

        private readonly static string RabbitHostNameKey = "rabbitmq:hostnames";
        private readonly static string RabbitPortKey = "rabbitmq:port";
        private readonly static string RabbitUsernameKey = "rabbitmq:username";
        private readonly static string RabbitPasswordKey = "rabbitmq:password";

        private readonly static string RedisConnectionStringKey = "redis:connectionString";

        private readonly static string MongoConnectionStringKey = "mongo:connectionString";

        public EduxWebApplicationFactory()
        {
            _msSqlContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-preview-ubuntu-22.04")
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

            _redisContainer = new RedisBuilder()
                .WithImage("redis:7.2.3")
                .WithCleanUp(true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
                .Build();

            _mongoDbContainer = new MongoDbBuilder()
                .WithImage("mongo-db:windowsservercore-ltsc2022")
                .WithCleanUp(true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHostBuilder) // Before application gets built
        {
            webHostBuilder.UseEnvironment("test");

            // SQL Server
            webHostBuilder.UseSetting(SqlServerConnectionStringKey, _msSqlContainer.GetConnectionString());

            // RabbitMQ
            var uri = new Uri(_rabbitMqContainer.GetConnectionString());
            webHostBuilder.UseSetting(RabbitHostNameKey, uri.Host);
            webHostBuilder.UseSetting(RabbitPortKey, uri.Port.ToString());
            webHostBuilder.UseSetting(RabbitUsernameKey, GetUsernameFromUserInfo(uri.UserInfo));
            webHostBuilder.UseSetting(RabbitPasswordKey, GetPasswordFromUserInfo(uri.UserInfo));

            // Redis
            webHostBuilder.UseSetting(RedisConnectionStringKey, _redisContainer.GetConnectionString());

            // MongoDB
            webHostBuilder.UseSetting(MongoConnectionStringKey, _mongoDbContainer.GetConnectionString());
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
