namespace Edux.Shared.Abstractions.Crypto
{
    public interface IRandomNumberGenerator
    {
        public string Generate(int length = 50, bool removeSpecialChars = true);
    }
}
