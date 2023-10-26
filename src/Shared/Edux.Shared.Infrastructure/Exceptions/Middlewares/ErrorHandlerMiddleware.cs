using Edux.Shared.Infrastructure.Exceptions.Dispatchers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Edux.Shared.Infrastructure.Exceptions.Middlewares
{
    internal class ErrorHandlerMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly IExceptionDispatcher _exceptionDispatcher;

        public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger,
            IExceptionDispatcher exceptionToResponseMapper)
        {
            _logger = logger;
            _exceptionDispatcher = exceptionToResponseMapper;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, exception.Message);
                await HandleErrorAsync(context, exception);
            }
        }

        private async Task HandleErrorAsync(HttpContext context, Exception exception)
        {
            var errorResponse = _exceptionDispatcher.Handle(exception);

            context.Response.StatusCode = (int)(errorResponse?.StatusCode ?? HttpStatusCode.InternalServerError);

            var response = errorResponse?.Response;
            if (response is null)
            {
                return;
            }

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
