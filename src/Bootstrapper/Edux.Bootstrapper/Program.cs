using Edux.Bootstrapper;
using Edux.Shared.Infrastructure;
using Edux.Shared.Infrastructure.Modules;
using Edux.Shared.Infrastructure.Logging;
using Edux.Shared.Infrastructure.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Configure IoC container.
builder.Host.LoadModuleSettings();
var _assemblies = ModuleLoader.LoadAssemblies(builder.Configuration);
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

// Configure the HTTP request pipeline.
app.UseInfrastructure();
foreach (var module in _modules)
{
    module.Use(app);
}

// Logging
app.UseCorrelationContextLogging();

var logger = app.Services.GetService<ILogger<Program>>();
logger.LogInformation($"Modules: {string.Join(", ", _modules.Select(x => x.Name))}");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseEndpoints(endpointRouteBuilder =>
{
    endpointRouteBuilder.MapControllers();
    endpointRouteBuilder.MapGet("/", () => "Edux API!");
    endpointRouteBuilder.MapModuleInfo();
    endpointRouteBuilder.MapLogLevelEndpoint("~/logging/level");
});

_assemblies.Clear();
_modules.Clear();

app.Run();

public partial class Program { } // Required by 'WebApplicationFactory' for integration testing