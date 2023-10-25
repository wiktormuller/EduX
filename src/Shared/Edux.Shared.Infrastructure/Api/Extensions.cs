using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Edux.Shared.Infrastructure.Api
{
    internal static class Extensions
    {
        public static IServiceCollection AddControllersWithOpenApi(this IServiceCollection services)
        {
            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddControllers()
                .ConfigureApplicationPartManager(manager =>
                {
                    // Thanks to this part there will not be any run-time classes from the disabled module
                    var removedParts = new List<ApplicationPart>();
                    foreach (var disabledModule in GetDisabledModules(services))
                    {
                        var parts = manager.ApplicationParts
                            .Where(part => part.Name.Contains(disabledModule, StringComparison.InvariantCultureIgnoreCase));
                        removedParts.AddRange(parts);
                    }

                    foreach (var part in removedParts)
                    {
                        manager.ApplicationParts.Remove(part);
                    }

                    // Thanks to this setting the bootstrapper can see internal controllers from modules
                    manager.FeatureProviders.Add(new InternalControllerFeatureProvider());
                });

            services
                .AddSwaggerGen(swaggerGenOptions =>
                {
                    swaggerGenOptions.CustomSchemaIds(x => x.FullName); // It's required, because there may be the same types in two different modules
                    swaggerGenOptions.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Edux API",
                        Version = "v1"
                    });
                    swaggerGenOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Please enter a valid JWT",
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                    swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
                });

            services.AddCors(cors =>
            {
                cors.AddPolicy("cors", x =>
                {
                    x.WithOrigins("*")
                        .WithHeaders("Content-Type", "Authorization")
                        .WithMethods("POST", "PUT", "DELETE");
                });
            });

            return services;
        }

        private static IEnumerable<string> GetDisabledModules(IServiceCollection services)
        {
            var disabledModules = new List<string>();
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                foreach (var (key, value) in configuration.AsEnumerable())
                {
                    if (!key.Contains(":module:enabled"))
                    {
                        continue;
                    }

                    if (!bool.Parse(value))
                    {
                        disabledModules.Add(key.Split(":")[0]);
                    }
                }
            }

            return disabledModules;
        }
    }
}
