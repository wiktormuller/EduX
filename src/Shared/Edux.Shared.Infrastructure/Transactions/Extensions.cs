using Edux.Shared.Abstractions.Commands;
using Edux.Shared.Abstractions.SharedKernel;
using Edux.Shared.Infrastructure.Transactions.Decorators;
using Edux.Shared.Infrastructure.Transactions.Registries;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Transactions
{
    internal static class Extensions
    {
        public static IServiceCollection AddTransactionalDecorators(this IServiceCollection services)
        {
            services.TryDecorate(typeof(ICommandHandler<>), typeof(TransactionalCommandHandlerDecorator<>));

            services.TryDecorate(typeof(IDomainEventHandler<>), typeof(TransactionalDomainEventHandlerDecorator<>));

            services.AddSingleton(new UnitOfWorkTypeRegistry());

            return services;
        }
    }
}
