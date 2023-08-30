using Edux.Modules.Users.Application.Contracts.Requests;
using Edux.Shared.Tests;
using System.Net.Http.Json;
using Shouldly;
using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Edux.Modules.Users.Tests.EndToEnd.Controllers
{
    public class UsersControllerTests : WebApiTestBase
    {
        public UsersControllerTests(WebApplicationFactory<Program> factory) : base(factory)
        {
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
            var httpResponseMessage = await Client.PostAsJsonAsync("users", signUpRequest, SerializerOptions);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.ShouldBeTrue();
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        }
    }
}
