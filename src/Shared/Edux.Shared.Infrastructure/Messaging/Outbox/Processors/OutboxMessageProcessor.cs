using Edux.Shared.Abstractions.Messaging.Publishers;
using Edux.Shared.Infrastructure.Messaging.Outbox.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Edux.Shared.Infrastructure.Messaging.Outbox.Processors
{
    internal sealed class OutboxMessageProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBusPublisher _busPublisher;
        private readonly OutboxOptions _outboxOptions;
        private readonly ILogger<OutboxMessageProcessor> _logger;
        private readonly TimeSpan _interval;
        private readonly string _type;
        private int _isProcessing; // Make processing outbox messages thread-safe

        public OutboxMessageProcessor(IServiceScopeFactory serviceScopeFactory,
            IBusPublisher busPublisher,
            OutboxOptions outboxOptions,
            ILogger<OutboxMessageProcessor> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _busPublisher = busPublisher;
            _outboxOptions = outboxOptions;
            _type = outboxOptions.Type;
            _logger = logger;
            _interval = TimeSpan.FromMilliseconds(outboxOptions.IntervalMilliseconds);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_outboxOptions.Enabled)
            {
                _logger.LogWarning("Outbox is disabled");
                return;
            }

            _logger.LogInformation($"Outbox is enabled, type: '{_type}', message processing every {_interval} ms.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Interlocked.Exchange(ref _isProcessing, 1) == 1)
                {
                    await Task.Delay(_interval, stoppingToken);
                    continue;
                }

                _logger.LogTrace("Started processing outbox messages...");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                using var scope = _serviceScopeFactory.CreateScope();

                try
                {
                    var outboxes = scope.ServiceProvider.GetServices<IMessageOutbox>();
                    var tasks = outboxes.Select(outbox => outbox.PublishUnsentAsync());
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    _logger.LogError("There was an error when processing outbox.");
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref _isProcessing, 0);
                    stopwatch.Stop();
                    _logger.LogTrace($"Finished processing outbox messages in {stopwatch.ElapsedMilliseconds} ms.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
