using Edux.Modules.Users.Application.Contracts.Requests;
using Shouldly;
using System.Net;
using Edux.Shared.Tests;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Edux.Shared.Abstractions.Auth;
using Edux.Modules.Users.Application.Contracts.Responses;

namespace Edux.Modules.Users.Tests.EndToEnd.Controllers
{
    public class UsersEndpointsTests : EduxTestBase
    {
        public UsersEndpointsTests(EduxWebApplicationFactory<Program> factory) : base(factory)
        {
        }

        [Fact]
        public async Task post_sign_up_when_passing_valid_request_then_should_returns_201_status_code()
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

        [Fact]
        public async Task post_sign_up_when_passing_invalid_request_then_should_returns_400_status_code()
        {
            // Arrange
            var signUpRequest = new SignUpRequest
            {
                Email = "incorrect-email-address",
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
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task post_sign_in_when_passing_valid_request_then_should_returns_201_status_code()
        {
            // Arrange
            // TODO: Prepare user in database
            var signInRequest = new SignInRequest
            {
                Email = "user1@email.com",
                Password = "Password123!"
            };

            // Act
            var httpResponseMessage = await PostAsync("users-module/users/sign-in", signInRequest);

            // Assert
            httpResponseMessage.IsSuccessStatusCode.ShouldBeTrue();
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
            
            var jwt = await httpResponseMessage.Content.ReadFromJsonAsync<JsonWebToken>();
            jwt.AccessToken.ShouldNotBeNullOrEmpty();
            jwt.RefreshToken.ShouldNotBeNullOrEmpty();
            jwt.Expires.ShouldBeGreaterThan(0);
            jwt.Id.ShouldNotBeNullOrEmpty();
            jwt.Role.ShouldNotBeNullOrEmpty();
            jwt.Email.ShouldBe("user1@email.com");
        }

        [Fact]
        public async Task post_sign_in_when_passing_invalid_request_then_should_returns_400_status_code()
        {
            // Arrange
            var signInRequest = new SignInRequest
            {
                Email = "incorrect-email-address",
                Password = "Password123!"
            };

            // Act
            var httpResponseMessage = await PostAsync("users-module/users/sign-in", signInRequest);

            // Assert
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task get_when_is_admin_then_should_returns_200_status_code()
        {
            // Arrange
            // TODO: Sign-In & prepapre user in Db

            // Act
            var httpResponseMessage = await GetAsync("users-module/users");

            // Assert
            httpResponseMessage.IsSuccessStatusCode.ShouldBeTrue();
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);

            var users = await httpResponseMessage.Content.ReadFromJsonAsync<IEnumerable<UserResponse>>();
            users.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task get_when_access_token_is_invalid_then_should_returns_401_status_code()
        {
            // Arrange
            // TODO: Pass incorrect access token

            // Act
            var httpResponseMessage = await GetAsync("users-module/users");

            // Assert
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task get_when_is_user_then_should_returns_403_status_code()
        {
            // Arrange
            // TODO: Sign-In as user

            // Act
            var httpResponseMessage = await GetAsync("users-module/users");

            // Assert
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task get_me_when_is_autorized_then_should_returns_200_status_code()
        {
            // Arrange
            // TODO: Sign-In as user

            // Act
            var httpResponseMessage = await GetAsync("users-module/users/me");

            // Assert
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);

            var userMe = await httpResponseMessage.Content.ReadFromJsonAsync<UserMeResponse>();
            userMe.Id.ShouldNotBe(Guid.Empty);
            userMe.Email.ShouldNotBeNullOrEmpty();
            userMe.Role.ShouldNotBeNullOrEmpty();
            userMe.IsActive.ShouldBe(true);
        }

        [Fact]
        public async Task get_me_when_access_token_is_invalid_then_should_returns_401_status_code()
        {
            // Arrange
            // TODO: Pass incorrect access token

            // Act
            var httpResponseMessage = await GetAsync("users-module/users/me");

            // Assert
            httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        // TODO: UseRefreshToken
        // TODO: RevokeRefreshToken
        // TODO: UpdateUser

        protected override void ConfigureServices(IServiceCollection services)
        {
        }
    }
}
