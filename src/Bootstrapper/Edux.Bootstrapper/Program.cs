using Edux.Bootstrapper;
using Edux.Shared.Infrastructure;
using Edux.Shared.Infrastructure.Modules;
using Edux.Shared.Infrastructure.Secrets;
using Edux.Shared.Infrastructure.Observability.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure IoC container.
builder.Host.LoadModuleSettings();
var _assemblies = ModuleLoader.LoadAssemblies(builder.Configuration);
_assemblies = _assemblies
    .Where(assembly => 
        !string.Equals(assembly.FullName, 
            "Microsoft.Data.SqlClient, Version=5.0.0.0, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5", 
            StringComparison.OrdinalIgnoreCase)) // https://github.com/dotnet/SqlClient/issues/1930
    .ToList();

var _modules = ModuleLoader.LoadModules(_assemblies);
builder.Services.AddInfrastructure(_assemblies, _modules);
foreach (var module in _modules)
{
    module.Register(builder.Services);
}

// Custom logging
builder.Host.InstallLogging();

// Vault integration
builder.InstallVault();

var app = builder.Build();

// Logging
app.UseCorrelationContextLogging();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"Modules: {string.Join(", ", _modules.Select(x => x.Name))}");

app.UseInfrastructure();
foreach (var module in _modules)
{
    module.Use(app);
}

_assemblies.Clear();
_modules.Clear();

app.Run();

public partial class Program { } // Required by 'WebApplicationFactory' for integration testing