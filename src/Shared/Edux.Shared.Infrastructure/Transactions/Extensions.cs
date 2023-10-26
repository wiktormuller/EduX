using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Infrastructure.Transactions.Decorators;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Transactions
{
    internal static class Extensions
    {
        public static IServiceCollection AddTransactionalDecorators(this IServiceCollection services)
        {
            services.TryDecorate(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));
            // TODO: Do the same for event handlers
            return services;
        }
    }
}
