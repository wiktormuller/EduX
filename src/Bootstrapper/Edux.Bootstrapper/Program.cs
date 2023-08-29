using Edux.Bootstrapper;
using Edux.Shared.Infrastructure;
using Edux.Shared.Infrastructure.Modules;

var builder = WebApplication.CreateBuilder(args);

// Configure IoC container.
builder.Host.LoadModuleSettings();
var _assemblies = ModuleLoader.LoadAssemblies(builder.Configuration);
var _modules = ModuleLoader.LoadModules(_assemblies);
builder.Services.AddInfrastructure(_assemblies);
foreach (var module in _modules)
{
    module.Register(builder.Services);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseInfrastructure();
foreach (var module in _modules)
{
    module.Use(app);
}

var logger = app.Services.GetService<ILogger<Program>>();
logger.LogInformation($"Modules: {string.Join(", ", _modules.Select(x => x.Name))}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

_assemblies.Clear();
_modules.Clear();

app.Run();

public partial class Program { } // Required by 'WebApplicationFactory' for integration testing