using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NetArchTest.Rules;
using System.Reflection;

namespace Edux.Architecture.Tests
{
    public class DomainLayerTests
    {
        [Fact]
        public void Users_Domain_Layer_Should_Never_Have_Any_Dependencies_To_Infrastructure_Application_And_Api()
        {
            // Arrange
            var applicationLayer = "Edux.Modules.Users.Application";
            var infrasatructureLayer = "Edux.Modules.Users.Infrastructure";
            var apiLayer = "Edux.Modules.Users.Api";
            var assemblyFromUsersDomain = Assembly.GetAssembly(typeof(Modules.Users.Core.Extensions));

            // Act

            var testResult = Types
                .InAssembly(assemblyFromUsersDomain)
                .ShouldNot()
                .HaveDependencyOnAll(applicationLayer, infrasatructureLayer, apiLayer)
                .GetResult();

            // Assert
            Assert.True(testResult.IsSuccessful);
        }

        [Fact]
        public void Users_Application_Layer_Should_Have_Any_Dependencies_To_Infrastructure_And_Api()
        {
            // Arrange
            var infrasatructureLayer = "Edux.Modules.Users.Infrastructure";
            var apiLayer = "Edux.Modules.Users.Api";
            var assemblyFromUsersApplication = Assembly.GetAssembly(typeof(Modules.Users.Application.Extensions));

            // Act
            var testResult = Types
                .InAssembly(assemblyFromUsersApplication)
                .ShouldNot()
                .HaveDependencyOnAll(infrasatructureLayer, apiLayer)
                .GetResult();

            // Assert
            Assert.True(testResult.IsSuccessful);
        }

        [Fact]
        public void Users_Controllers_Should_Never_Depends_Directly_On_Repositories()
        {
            // Arrange
            var assemblyFromUsersApi = Assembly.GetAssembly(typeof(Modules.Users.Api.UsersModule));

            // Act
            var testResult = Types
                .InAssembly(assemblyFromUsersApi)
                .That()
                .HaveNameEndingWith("Controller")
                .ShouldNot()
                .HaveDependencyOnAny("Edux.Modules.Users.Infrastructure.EF.Repositories")
                .GetResult();

            // Assert
            Assert.True(testResult.IsSuccessful);
        }
    }
}
