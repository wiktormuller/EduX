using Edux.Shared.Abstractions.Crypto;
using System.Security.Cryptography;

namespace Edux.Shared.Infrastructure.Crypto
{
    internal sealed class RandomNumberGenerator : IRandomNumberGenerator
    {
        private static readonly string[] SpecialChars = new[] { "/", "\\", "=", "+", "?", ":", "&" };

        public string Generate(int length = 50, bool removeSpecialChars = true)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes); // Fulfill array of bytes with random values
            var result = Convert.ToBase64String(bytes);

            return removeSpecialChars
                ? SpecialChars.Aggregate(result, (current, chars) => current.Replace(chars, string.Empty))
                : result;
        }
    }
}
