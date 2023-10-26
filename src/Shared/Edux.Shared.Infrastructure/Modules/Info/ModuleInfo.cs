namespace Edux.Shared.Infrastructure.Modules.Info
{
    internal record ModuleInfo(string Name, string Path, IEnumerable<string> Policies);
}
