using BookIt.Domain.Members;

namespace BookIt.Application.Abstractions;

public interface ITokenService
{
    TokenPair CreateSession(Member member);
    AccessToken CreateAccessToken(Member member);
    RefreshTokenPayload ValidateRefreshToken(string refreshToken);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

public sealed record TokenPair(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);

public sealed record RefreshTokenPayload(Guid MemberId, DateTimeOffset ExpiresAt);
