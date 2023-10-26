using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Edux.Shared.Infrastructure.Messaging.Outbox.Processors
{
    internal sealed class OutboxMessageCleanupProcessor : BackgroundService
    {
        private readonly OutboxOptions _outboxOptions;
        private readonly IClock _clock;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OutboxMessageCleanupProcessor> _logger;
        private readonly TimeSpan _interval;
        private int _isProcessing;

        public OutboxMessageCleanupProcessor(OutboxOptions outboxOptions,
            IClock clock,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<OutboxMessageCleanupProcessor> logger)
        {
            _outboxOptions = outboxOptions;
            _clock = clock;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _interval = TimeSpan.FromMilliseconds(outboxOptions.OutboxCleanupIntervalMilliseconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_outboxOptions.Enabled)
            {
                _logger.LogWarning("Outbox is disabled");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Interlocked.Exchange(ref _isProcessing, 1) == 1)
                {
                    await Task.Delay(_interval, stoppingToken);
                }

                _logger.LogTrace("Started cleaning up outbox messages...");
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var outboxes = scope.ServiceProvider.GetServices<IMessageOutbox>();
                    var tasks = outboxes.Select(outbox => outbox.CleanupAsync(_clock.CurrentDate().Subtract(_interval)));
                    await Task.WhenAll(tasks);
                }
                catch(Exception ex)
                {
                    _logger.LogError("There was an error when processing outbox.");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref _isProcessing, 0);
                    stopwatch.Stop();
                    _logger.LogTrace($"Finished cleaning up outbox messages in {stopwatch.ElapsedMilliseconds} ms.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
