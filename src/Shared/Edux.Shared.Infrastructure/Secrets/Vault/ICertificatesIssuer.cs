using System.Security.Cryptography.X509Certificates;

namespace Edux.Shared.Infrastructure.Secrets.Vault
{
    internal interface ICertificatesIssuer
    {
        Task<X509Certificate2> IssueAsync();
    }
}
