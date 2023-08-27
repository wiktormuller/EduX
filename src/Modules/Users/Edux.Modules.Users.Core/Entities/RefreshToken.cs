using Edux.Modules.Users.Core.Exceptions;
using Edux.Shared.Abstractions.Kernel.Types;

namespace Edux.Modules.Users.Core.Entities
{
    public class RefreshToken : AggregateRoot
    {
        public AggregateId UserId { get; private set; }
        public string Token { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public bool Revoked => RevokedAt.HasValue;

        protected RefreshToken()
        {
        }

        public RefreshToken(AggregateId id, AggregateId userId, string token, DateTime createdAt,
            DateTime? revokedAt = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new EmptyRefreshTokenException();
            }

            if (token.Length < 5 && token.Length > 256)
            {
                throw new InvalidRefreshTokenException();
            }

            Id = id;
            UserId = userId;
            Token = token;
            CreatedAt = createdAt;
            RevokedAt = revokedAt;
        }

        public void Revoke(DateTime revokedAt)
        {
            if (Revoked)
            {
                throw new RevokedRefreshTokenException();
            }

            RevokedAt = revokedAt;
        }
    }
}
