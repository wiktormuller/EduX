﻿using Edux.Shared.Abstractions.Exceptions;
using Edux.Shared.Abstractions.SharedKernel.Exceptions;
using System.Net;

namespace Edux.Shared.Infrastructure.Exceptions.Mappers
{
    internal sealed class DefaultExceptionToResponseMapper : IExceptionToResponseMapper
    {
        public ExceptionResponse Map(Exception exception)
        {
            return exception switch
            {
                DomainException ex =>
                    new ExceptionResponse(new ErrorResponse(new Error(ex.Code, ex.Message)),
                    HttpStatusCode.BadRequest),

                Abstractions.SharedKernel.Exceptions.ApplicationException ex =>
                    new ExceptionResponse(new ErrorResponse(new Error(ex.Code, ex.Message)),
                    HttpStatusCode.BadRequest),

                _ => new ExceptionResponse(new ErrorResponse(new Error("error", "There was an error.")),
                    HttpStatusCode.InternalServerError)
            };
        }

        private record ErrorResponse(params Error[] errors);

        private record Error(string Code, string Message);
    }
}
