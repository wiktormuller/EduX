﻿namespace Edux.Shared.Abstractions.Modules
{
    public interface IModuleClient
    {
        Task SendAsync(string path, object request, CancellationToken cancellationToken = default);

        Task<TResult> SendAsync<TResult>(string path, object request, CancellationToken cancellationToken = default) 
            where TResult : class;
    }
}
