using Edux.Modules.Users.Application.Contracts.Requests;
using Shouldly;
using System.Net;
using Edux.Shared.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Modules.Users.Tests.EndToEnd.Controllers
{
    public class UsersControllerTests : EduxTestBase
    {
        public UsersControllerTests(EduxWebApplicationFactory<Program> factory) : base(factory)
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
            var httpResponseMessage = await PostAsync("users-module/users/sign-up", signUpRequest);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.ShouldBeTrue();
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
