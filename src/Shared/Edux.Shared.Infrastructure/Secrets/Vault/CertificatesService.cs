﻿using Edux.Shared.Abstractions.Secrets;
using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;

namespace Edux.Shared.Infrastructure.Secrets.Vault
{
    internal sealed class CertificatesService : ICertificatesService
    {
        private static readonly ConcurrentDictionary<string, X509Certificate2> Certificates = new();

        public IReadOnlyDictionary<string, X509Certificate2> All => Certificates;

        public X509Certificate2 Get(string name) => Certificates.TryGetValue(name, out var cert) ? cert : null;

        public void Set(string name, X509Certificate2 certificate)
        {
            Certificates.TryRemove(name, out _);
            Certificates.TryAdd(name, certificate);
        }
    }
}
