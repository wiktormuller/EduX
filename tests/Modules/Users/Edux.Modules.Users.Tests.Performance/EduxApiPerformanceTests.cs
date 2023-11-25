using NBomber.CSharp;
using NBomber.Http.CSharp;
using Shouldly;
using Xunit.Abstractions;

namespace Edux.Modules.Users.Tests.Performance
{
    public class EduxApiPerformanceTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public EduxApiPerformanceTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        //[Fact]
        //public void root_endpoint_from_bootstrapper_handle_at_least_100_requests_per_seconds()
        //{
        //    // Arrange
        //    const string url = "http://localhost:5000/";

        //    using var httpClient = new HttpClient();

        //    const int expectedRequestsPerSeconds = 100;
        //    const int durationSeconds = 5;

        //    var getRootScenario = Scenario.Create("get root scenario", async context =>
        //    {
        //        try
        //        {
        //            var request = Http.CreateRequest("GET", url);

        //            var response = await Http.Send(httpClient, request);
        //            return response;
        //        }
        //        catch
        //        {
        //            return Response.Fail();
        //        }
        //    })
        //    .WithWarmUpDuration(TimeSpan.FromSeconds(5))
        //    .WithLoadSimulations(Simulation.KeepConstant(copies: expectedRequestsPerSeconds, during: TimeSpan.FromSeconds(durationSeconds)));

        //    // Act
        //    var stats = NBomberRunner
        //        .RegisterScenarios(getRootScenario)
        //        .Run();

        //    // Assert
        //    _outputHelper.WriteLine($"OK: {stats.AllOkCount}, FAILED: {stats.AllFailCount}.");
        //    stats.AllOkCount.ShouldBeGreaterThanOrEqualTo(durationSeconds * expectedRequestsPerSeconds);
        //}
    }
}
