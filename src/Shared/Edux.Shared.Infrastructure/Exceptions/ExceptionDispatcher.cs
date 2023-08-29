using Edux.Shared.Abstractions.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Exceptions
{
    internal sealed class ExceptionDispatcher : IExceptionDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public ExceptionDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ExceptionResponse Handle(Exception exception)
        {
            using var scope = _serviceProvider.CreateScope();
            var mappers = _serviceProvider.GetServices<IExceptionToResponseMapper>().ToArray();
            var nonDefaultMappers = mappers
                .Where(m => m is not DefaultExceptionToResponseMapper);

            var result = nonDefaultMappers
                .Select(m => m.Map(exception))
                .SingleOrDefault(r => r is not null);

            if (result is not null)
            {
                return result;
            }

            // Fallback to default
            var defaultMapper = mappers.SingleOrDefault(m => m is DefaultExceptionToResponseMapper);

            return defaultMapper?.Map(exception);
        }
    }
}
