using Edux.Shared.Abstractions.Secrets;
using Edux.Shared.Abstractions.Time;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VaultSharp;

namespace Edux.Shared.Infrastructure.Secrets.Vault
{
    internal sealed class VaultHostedService : BackgroundService
    {
        private readonly IVaultClient _client;
        private readonly ILeaseService _leaseService;
        private readonly ICertificatesService _certificatesService;
        private readonly ICertificatesIssuer _certificatesIssuer;
        private readonly ILogger<VaultHostedService> _logger;
        private readonly VaultOptions _options;
        private readonly IClock _clock;

        private readonly int _interval;

        public VaultHostedService(IVaultClient client,
            ILeaseService leaseService,
            ICertificatesService certificatesService,
            ICertificatesIssuer certificatesIssuer,
            ILogger<VaultHostedService> logger,
            VaultOptions options,
            IClock clock)
        {
            _client = client;
            _leaseService = leaseService;
            _certificatesService = certificatesService;
            _certificatesIssuer = certificatesIssuer;
            _logger = logger;
            _options = options;

            _interval = _options.RenewalsInterval <= 0
                ? 10
                : _options.RenewalsInterval;
            _clock = clock;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                return;
            }

            if ((_options.PKI is null || !_options.PKI.Enabled) &&
                (_options.Lease is null || _options.Lease.All(l => !l.Value.Enabled) ||
                    !_options.Lease.Any(l => l.Value.AutoRenewal)))
            {
                return;
            }

            _logger.LogInformation($"Vault lease renewals will be processed every {_interval} seconds.");

            var interval = TimeSpan.FromSeconds(_interval);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = _clock.CurrentDate();
                var nextIterationAt = now.AddSeconds(2 * _interval);

                if (_options.PKI is not null && _options.PKI.Enabled)
                {
                    foreach (var (role, cert) in _certificatesService.All)
                    {
                        if (cert.NotAfter > nextIterationAt)
                        {
                            continue;
                        }

                        _logger.LogInformation($"Issuing a certificate for: '{role}'.");

                        var certificate = await _certificatesIssuer.IssueAsync();
                        _certificatesService.Set(role, certificate);
                    }
                }

                foreach (var (key, lease) in _leaseService.All.Where(l => l.Value.AutoRenewal))
                {
                    if (lease.ExpiryAt > nextIterationAt)
                    {
                        continue;
                    }

                    _logger.LogInformation($"Renewing a lease with ID: '{lease.Id}', for: '{key}', duration: {lease.Duration} seconds.");

                    var beforeRenew = _clock.CurrentDate();
                    var renewedLease = await _client.V1.System.RenewLeaseAsync(lease.Id, lease.Duration);
                    lease.Refresh(renewedLease.LeaseDurationSeconds - (lease.ExpiryAt - beforeRenew).TotalSeconds);
                }

                await Task.Delay(interval.Subtract(_clock.CurrentDate() - now), stoppingToken);
            }

            if (!_options.RevokeLeaseOnShutdown)
            {
                return;
            }

            foreach (var (key, lease) in _leaseService.All)
            {
                _logger.LogInformation($"Revoking a lease with ID: '{lease.Id}', for: '{key}'.");
                await _client.V1.System.RevokeLeaseAsync(lease.Id);
            }
        }
    }
}
