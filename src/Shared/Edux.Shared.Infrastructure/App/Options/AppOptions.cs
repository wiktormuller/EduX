namespace Edux.Shared.Infrastructure.App.Options
{
    internal sealed class AppOptions
    {
        public string Name { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}
