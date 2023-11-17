using Edux.Shared.Abstractions.Modules;
using System.Reflection;

namespace Edux.Bootstrapper
{
    internal static class ModuleLoader
    {
        const string MODULE_PART = "Edux.Modules.";

        public static IList<Assembly> LoadAssemblies(IConfiguration configuration)
        {
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .ToList();

            var locationsOfAssemblies = assemblies.Where(a => !a.IsDynamic)
                .Select(a => a.Location)
                .ToArray();

            // All dlls that aren't associated with Bootstrapper AppDomain (the dlls of our modules are here and much more)
            var dllFilesNames = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Where(fileName => !locationsOfAssemblies.Contains(fileName, StringComparer.InvariantCultureIgnoreCase))
                .ToList();

            var dllFilesOfDisabledModules = new List<string>();
            foreach (var dllFileName in dllFilesNames)
            {
                if (!dllFileName.Contains(MODULE_PART))
                {
                    continue;
                }

                var moduleName = dllFileName // For example "Edux.Modules.Users.BaseController"
                    .Split(MODULE_PART)[1]
                    .Split(".")[0].ToLowerInvariant();
                var enabled = configuration.GetValue<bool>($"{moduleName}:module:enabled");
                if (!enabled)
                {
                    dllFilesOfDisabledModules.Add(dllFileName);
                }
            }

            foreach (var dllFileOfDisabledModules in dllFilesOfDisabledModules)
            {
                dllFilesNames.Remove(dllFileOfDisabledModules);
            }

            foreach (var dllFileName in dllFilesNames)
            {
                assemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(dllFileName)));
            }

            return assemblies;
        }

        public static IList<IModule> LoadModules(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes().Where(t => !t.FullName.Equals("")))
                .Where(type => typeof(IModule).IsAssignableFrom(type) && !type.IsInterface)
                .OrderBy(type => type.Name)
                .Select(Activator.CreateInstance)
                .Cast<IModule>()
                .ToList();
        }
    }
}
