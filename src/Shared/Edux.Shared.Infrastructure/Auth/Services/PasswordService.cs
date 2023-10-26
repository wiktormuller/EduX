using Edux.Shared.Abstractions.Auth;
using Microsoft.AspNetCore.Identity;

namespace Edux.Shared.Infrastructure.Auth.Services
{
    internal sealed class PasswordService : IPasswordService
    {
        private readonly IPasswordHasher<PasswordService> _passwordHasher;

        public PasswordService(IPasswordHasher<PasswordService> passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public string Hash(string password)
        {
            return _passwordHasher.HashPassword(this, password);
        }

        public bool IsValid(string hash, string password)
        {
            return _passwordHasher.VerifyHashedPassword(this, hash, password) != PasswordVerificationResult.Failed;
        }
    }
}
