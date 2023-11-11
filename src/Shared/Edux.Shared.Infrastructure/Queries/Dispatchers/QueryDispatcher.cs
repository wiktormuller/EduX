using Edux.Shared.Abstractions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Edux.Shared.Infrastructure.Queries.Dispatchers
{
    internal sealed class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var queryType = query.GetType();
            var resultType = typeof(TResult?);
            var queryHandlerOpenType = typeof(IQueryHandler<,>);
            var queryHandlerType = queryHandlerOpenType.MakeGenericType(queryType, resultType);

            var handlerInstance = scope.ServiceProvider.GetRequiredService(queryHandlerType); // It's object type
            var handlerMethodName = nameof(IQueryHandler<IQuery<TResult?>, TResult?>.HandleAsync);

            return await (Task<TResult?>)queryHandlerType
                .GetMethod(handlerMethodName)! // Or "HandleAsync"
                .Invoke(handlerInstance, new[] { query })!;
        }
    }
}
