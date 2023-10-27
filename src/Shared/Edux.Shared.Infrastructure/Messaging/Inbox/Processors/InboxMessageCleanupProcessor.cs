using Edux.Shared.Abstractions.Time;
using Edux.Shared.Infrastructure.Messaging.Inbox.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Edux.Shared.Infrastructure.Messaging.Inbox.Processors
{
    internal sealed class InboxMessageCleanupProcessor : BackgroundService
    {
        private readonly IServiceProvider _servicesProvider;
        private readonly IClock _clock;
        private readonly ILogger<InboxMessageCleanupProcessor> _logger;
        private readonly InboxOptions _inboxOptions;

        private int _isProcessing;
        private readonly TimeSpan _interval;

        public InboxMessageCleanupProcessor(IServiceProvider servicesProvider,
            IClock clock,
            ILogger<InboxMessageCleanupProcessor> logger,
            InboxOptions inboxOptions)
        {
            _servicesProvider = servicesProvider;
            _clock = clock;
            _logger = logger;
            _inboxOptions = inboxOptions;
            _interval = inboxOptions.CleanupInterval ?? TimeSpan.FromHours(1);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_inboxOptions.Enabled)
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                if (Interlocked.Exchange(ref _isProcessing, 1) == 1) // Make processing outbox messages thread-safe
                {
                    await Task.Delay(_interval, stoppingToken);
                    continue;
                }

                _logger.LogInformation("Started cleaning up inbox messages...");
                
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await using var scope = _servicesProvider.CreateAsyncScope();
                try
                {
                    var inbox = scope.ServiceProvider.GetRequiredService<IMessageInbox>();
                    await inbox.CleanUpAsync(_clock.CurrentDate().Subtract(_interval), stoppingToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError("There was an error when processing inbox.");
                    _logger.LogError(exception, exception.Message);
                }
                finally
                {
                    Interlocked.Exchange(ref _isProcessing, 0);
                    stopwatch.Stop();
                    _logger.LogInformation($"Finished cleaning up inbox messages in {stopwatch.ElapsedMilliseconds} ms.");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}
