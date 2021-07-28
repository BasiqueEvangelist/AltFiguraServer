using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace AltFiguraServer.LoginServer
{
    public static class SessionUtils
    {
        private const string ISSUER = "AltFiguraServer";
        private const string AUDIENCE = "AltFiguraServer";

        private static readonly RSA serverKeyPair = RSA.Create(1024);
        private static readonly RsaSecurityKey serverKey = new(serverKeyPair);
        private static readonly JwtSecurityTokenHandler tokenHandler = new();
        private static readonly TokenValidationParameters validationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = ISSUER,
            ValidateAudience = true,
            ValidAudience = AUDIENCE,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = serverKey
        };
        private static readonly JwtHeader tokenHeader = new(new SigningCredentials(serverKey, SecurityAlgorithms.RsaSha512));

        public static string MintToken(Guid userUuid)
        {
            long now = (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;

            JwtPayload payload = new(new Claim[] {
                new(JwtRegisteredClaimNames.Iss, ISSUER),
                new(JwtRegisteredClaimNames.Aud, AUDIENCE),
                new(JwtRegisteredClaimNames.Sub, userUuid.ToString()),
                new(JwtRegisteredClaimNames.Iat, now.ToString()),
                new(JwtRegisteredClaimNames.Exp, (now + 60 * 30).ToString()),
            });
            JwtSecurityToken token = new(tokenHeader, payload);
            return tokenHandler.WriteToken(token);
        }

        public static bool ValidateToken(string token, out IEnumerable<Claim> claims)
        {
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                claims = ((JwtSecurityToken)validatedToken).Claims;
                return true;
            }
            catch (Exception)
            {
                claims = null;
                return false;
            }
        }
    }
}