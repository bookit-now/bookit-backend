using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookIt.Application.Abstractions;
using BookIt.Application.Authentication;
using BookIt.Domain.Members;
using Microsoft.IdentityModel.Tokens;

namespace BookIt.Infrastructure.Authentication;

internal sealed class JwtTokenService(JwtOptions options, TimeProvider timeProvider) : ITokenService
{
    private const string TokenTypeClaim = "token_type";
    private const string AccessTokenType = "access";
    private const string RefreshTokenType = "refresh";

    private readonly JwtSecurityTokenHandler _handler = new() { MapInboundClaims = false };
    private readonly SymmetricSecurityKey _signingKey = new(Encoding.UTF8.GetBytes(options.SigningKey));

    public TokenPair CreateSession(Member member)
    {
        var accessToken = CreateAccessToken(member);
        var refreshExpiresAt = GetUtcNow().AddDays(options.RefreshTokenDays);
        var refreshToken = CreateToken(member, options.RefreshAudience, RefreshTokenType, refreshExpiresAt);

        return new TokenPair(
            accessToken.Value,
            accessToken.ExpiresAt,
            refreshToken,
            refreshExpiresAt);
    }

    public AccessToken CreateAccessToken(Member member)
    {
        var expiresAt = GetUtcNow().AddMinutes(options.AccessTokenMinutes);
        return new AccessToken(
            CreateToken(member, options.AccessAudience, AccessTokenType, expiresAt),
            expiresAt);
    }

    public RefreshTokenPayload ValidateRefreshToken(string refreshToken)
    {
        try
        {
            var principal = _handler.ValidateToken(refreshToken, CreateValidationParameters(options.RefreshAudience), out var token);
            var tokenType = principal.FindFirstValue(TokenTypeClaim);
            var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (tokenType != RefreshTokenType ||
                !Guid.TryParse(subject, out var memberId) ||
                token is not JwtSecurityToken jwtToken)
            {
                throw new InvalidRefreshTokenException();
            }

            return new RefreshTokenPayload(memberId, new DateTimeOffset(jwtToken.ValidTo, TimeSpan.Zero));
        }
        catch (Exception exception) when (
            exception is SecurityTokenException or ArgumentException)
        {
            throw new InvalidRefreshTokenException();
        }
    }

    private string CreateToken(Member member, string audience, string tokenType, DateTimeOffset expiresAt)
    {
        var now = GetUtcNow();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, member.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, member.Email),
            new Claim(JwtRegisteredClaimNames.Name, member.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(TokenTypeClaim, tokenType)
        };

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return _handler.WriteToken(token);
    }

    private TokenValidationParameters CreateValidationParameters(string audience) => new()
    {
        ValidateIssuer = true,
        ValidIssuer = options.Issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = _signingKey,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        RequireSignedTokens = true,
        ClockSkew = TimeSpan.FromSeconds(30),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
    };

    private DateTimeOffset GetUtcNow() =>
        DateTimeOffset.FromUnixTimeSeconds(timeProvider.GetUtcNow().ToUnixTimeSeconds());
}
