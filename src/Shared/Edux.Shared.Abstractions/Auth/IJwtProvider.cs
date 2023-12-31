﻿namespace Edux.Shared.Abstractions.Auth
{
    public interface IJwtProvider
    {
        JsonWebToken CreateToken(string userId, string email, string role, string? audience = null, 
            IDictionary<string, IEnumerable<string>>? claims = null);

        JsonWebTokenPayload? GetTokenPayload(string accessToken);
    }
}
