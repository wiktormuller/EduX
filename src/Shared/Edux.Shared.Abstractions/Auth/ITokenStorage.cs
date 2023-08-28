namespace Edux.Shared.Abstractions.Auth
{
    public interface ITokenStorage
    {
        void Set(JsonWebToken jwt);
        JsonWebToken? Get();
    }
}
