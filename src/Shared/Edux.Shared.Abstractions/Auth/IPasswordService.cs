namespace Edux.Shared.Abstractions.Auth
{
    public interface IPasswordService
    {
        bool IsValid(string hash, string password);
        string Hash(string password);
    }
}
