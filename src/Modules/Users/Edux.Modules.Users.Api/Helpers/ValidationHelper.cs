using FluentValidation;
using Google.Protobuf.Reflection;
using Grpc.Net.Client.Balancer;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.ComponentModel.DataAnnotations;
using System;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Edux.Modules.Users.Api.Helpers
{
    internal class ValidationHelper
    {
        public static EndpointFilterDelegate ValidationFilterFactory(EndpointFilterFactoryContext context,
            EndpointFilterDelegate next)
        {
            var validationDescriptors = GetValidators(context.MethodInfo, context.ApplicationServices);

            if (validationDescriptors.Any())
            {
                // We only add the filter to an endpoint if there are any validators
                // So, the validators need to be registered as singletons.
                // Thanks to the factory (instead of direct EndpointFilters) we don't need to resolve the validators per every request

                // This factory, invoked per endpoint at startup does the following:
                // Checks the endpoint delegate and looks for any arguments decorated with the[Validate] attribute
                // Attempts to resolve the appropriate Fluent Validation validator type e.g.IValidator < RegisterCustomerRequest >
                // Creates a descriptor that represents the argument type, its position and resolved validator
                // If any descriptors are returned, register an endpoint filter delegate that validates the endpoint arguments using the appropriate validators
                return invocationContext => Validate(validationDescriptors, invocationContext, next);
            }

            // There aren't any validators.
            return invocationContext => next(invocationContext);
        }

        private static async ValueTask<object?> Validate(IEnumerable<ValidationDescriptor> validationDescriptors,
            EndpointFilterInvocationContext invocationContext,
            EndpointFilterDelegate next)
        {
            foreach (var descriptor in validationDescriptors)
            {
                var argument = invocationContext.Arguments[descriptor.ArgumentIndex];

                if (argument is not null)
                {
                    var validationResult = await descriptor.Validator.ValidateAsync(
                        new ValidationContext<object>(argument)
                    );

                    if (!validationResult.IsValid)
                    {
                        return Results.ValidationProblem(validationResult.ToDictionary(),
                            statusCode: StatusCodes.Status400BadRequest);
                    }
                }
            }

            return await next.Invoke(invocationContext);
        }

        private static IEnumerable<ValidationDescriptor> GetValidators(MethodInfo methodInfo,
            IServiceProvider serviceProvider)
        {
            var parameters = methodInfo.GetParameters();

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];

                if (parameter.GetCustomAttribute<ValidateAttribute>() is not null)
                {
                    var validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

                    // Note that FluentValidation validators needs to be registered as singleton
                    var validator = serviceProvider.GetService(validatorType) as IValidator;

                    if (validator is not null)
                    {
                        yield return new ValidationDescriptor
                        {
                            ArgumentIndex = i,
                            ArgumentType = parameter.ParameterType,
                            Validator = validator
                        };
                    }
                }
            }
        }
    }
}
