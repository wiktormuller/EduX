using Edux.Shared.Abstractions.Contexts;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace Edux.Shared.Infrastructure.Contexts
{
    internal sealed class ContextProvider : IContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IContextAccessor _contextAccessor;

        public ContextProvider(IHttpContextAccessor httpContextAccessor,
            IContextAccessor contextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _contextAccessor = contextAccessor;
        }

        public IContext Current()
        {
            if (_contextAccessor.Context is not null)
            {
                return _contextAccessor.Context;
            }

            IContext context;
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext is not null) // In scope of HTTP request
            {
                context = new Context(httpContext);
            }
            else // Out of HTTP request scope
            {
                context = new Context(Guid.NewGuid(), ActivityTraceId.CreateRandom().ToString());
            }

            _contextAccessor.Context = context;

            return context;
        }
    }
}
