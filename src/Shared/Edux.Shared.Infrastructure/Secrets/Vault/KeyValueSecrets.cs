using Edux.Shared.Abstractions.Secrets;
using Edux.Shared.Abstractions.Serializers;
using System.Text.Json;
using VaultSharp;

namespace Edux.Shared.Infrastructure.Secrets.Vault
{
    internal sealed class KeyValueSecrets : IKeyValueSecrets
    {
        private readonly IVaultClient _client;
        private readonly VaultOptions _options;

        public KeyValueSecrets(IVaultClient client, 
            VaultOptions options)
        {
            _client = client;
            _options = options;
        }

        public async Task<T> GetAsync<T>(string path)
            => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(await GetAsync(path)));

        public async Task<IDictionary<string, object>> GetAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Vault KV secret path can not be empty.");
            }

            try
            {
                switch (_options.KV.EngineVersion)
                {
                    case 1:
                        var secretV1 = await _client.V1.Secrets.KeyValue.V1.ReadSecretAsync(path,
                            _options.KV.MountPoint);
                        return secretV1.Data;
                    case 2:
                        var secretV2 = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path,
                            _options.KV.Version, _options.KV.MountPoint);
                        return secretV2.Data.Data;
                    default:
                        throw new ArgumentException($"Invalid KV engine version: {_options.KV.EngineVersion}.");
                }
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Getting Vault secret for path: '{path}' caused an error. " +
                                         $"{exception.Message}");
            }
        }
    }
}
