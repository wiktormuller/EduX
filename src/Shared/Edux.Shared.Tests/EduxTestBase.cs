using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.Text.Json;
using Xunit;
using Edux.Shared.Tests.Helpers;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Edux.Shared.Tests
{
    public class EduxTestBase : IClassFixture<EduxWebApplicationFactory<Program>>
    {
        protected HttpClient Client { get; private set; }

        protected readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly EduxWebApplicationFactory<Program> _eduxWebApplicationFactory;

        protected EduxTestBase(EduxWebApplicationFactory<Program> eduxWebApplicationFactory)
        {
            _eduxWebApplicationFactory = eduxWebApplicationFactory;

            Client = _eduxWebApplicationFactory
                .WithWebHostBuilder(configuration => configuration.ConfigureServices(ConfigureServices))
                .CreateClient();
        }

        protected Task<HttpResponseMessage> GetAsync(string endpoint)
            => Client.GetAsync(endpoint);

        protected Task<T?> GetJsonAsync<T>(string endpoint)
            => Client.GetFromJsonAsync<T>(endpoint);

        protected Task<HttpResponseMessage> PostAsync<T>(string endpoint, T payload)
            => Client.PostAsJsonAsync(endpoint, payload, SerializerOptions);

        protected Task<HttpResponseMessage> PutAsync<T>(string endpoint, T payload)
            => Client.PutAsJsonAsync(endpoint, payload, SerializerOptions);

        protected Task<HttpResponseMessage> DeleteAsync(string endpoint)
            => Client.DeleteAsync(endpoint);

        protected void Authenticate(Guid userId, string email, string role, 
            IDictionary<string, IEnumerable<string>>? claims = null)
        {
            var jwt = AuthHelper.CreateJwtToken(userId, email, role, claims: claims);

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        }

        protected virtual void ConfigureServices(IServiceCollection services)
        {
            // Template method (hook) when we want to override some service registration
        }
    }
}
