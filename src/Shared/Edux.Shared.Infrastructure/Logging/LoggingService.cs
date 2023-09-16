﻿namespace Edux.Shared.Infrastructure.Logging
{
    internal interface ILoggingService
    {
        public void SetLoggingLevel(string logEventLevel)
        {
            Extensions.LoggingLevelSwitch.MinimumLevel = Extensions.GetLogEventLevel(logEventLevel);
        }
    }
    internal class LoggingService : ILoggingService { }
}
