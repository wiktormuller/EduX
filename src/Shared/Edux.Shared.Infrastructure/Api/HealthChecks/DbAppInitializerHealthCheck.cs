using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Edux.Shared.Infrastructure.Api.HealthChecks
{
    internal sealed class DbAppInitializerHealthCheck : IHealthCheck
    {
        private volatile bool _isReady;

        // Separate readiness and liveness probes
        // In some hosting scenarios, a pair of health checks is used to distinguish two app states:
        // Readiness indicates if the app is running normally but isn't ready to receive requests.
        // Liveness indicates if an app has crashed and must be restarted.
        public bool StartupCompleted // Separate readiness and liveness probes
        {
            get => _isReady;
            set => _isReady = value;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            if (StartupCompleted)
            {
                return Task.FromResult(HealthCheckResult.Healthy("The DpAppInitializer task has completed."));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("That DbAppInitializer task is still running."));
        }
    }
}
