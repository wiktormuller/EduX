using Microsoft.Extensions.Configuration;

namespace Edux.Shared.Tests.Helpers
{
    public static class OptionsHelper
    {
        private const string AppSettings = "appsettings.test.json";

        public static TOptions GetOptions<TOptions>(string sectionName) where TOptions : class, new()
        {
            var options = new TOptions();
            var configuration = GetConfigurationRoot();
            var section = configuration.GetSection(sectionName);
            section.Bind(options);

            return options;
        }

        public static IConfigurationRoot GetConfigurationRoot()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(AppSettings, true)
                .AddEnvironmentVariables()
                .Build();
        }
    }
}
