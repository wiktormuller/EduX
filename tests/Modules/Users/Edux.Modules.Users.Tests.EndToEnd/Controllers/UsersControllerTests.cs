using Edux.Modules.Users.Application.Contracts.Requests;
using System.Net.Http.Json;
using Shouldly;
using System.Net;
using Edux.Shared.Tests;

namespace Edux.Modules.Users.Tests.EndToEnd.Controllers
{
    public class UsersControllerTests : IClassFixture<EduxWebApplicationFactory<Program>>
    {
        private readonly EduxWebApplicationFactory<Program> _factory;

        public UsersControllerTests(EduxWebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task post_sign_up_given_valid_request_should_succeed()
        {
            // Arrange
            var signUpRequest = new SignUpRequest
            {
                Email = "user1@email.com",
                Password = "Password123!",
                Role = "user",
                Username = "user1",
                Claims = new Dictionary<string, IEnumerable<string>>()
                {
                    ["permissions"] = new[] { "users" }
                }
            };

            // Act
            var httpResponseMessage = await _factory.CreateClient()
                .PostAsJsonAsync("users-module/users/sign-up", signUpRequest, _factory.SerializerOptions);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.ShouldBeTrue();
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        }
    }
}
