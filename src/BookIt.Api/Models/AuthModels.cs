using BookIt.Application.Authentication;

namespace BookIt.Api.Models;

public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record MemberResponse(Guid Id, string Name, string Email);

public sealed record AuthenticationResponse(
    MemberResponse Member,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt)
{
    public static AuthenticationResponse FromResult(AuthenticationResult result) => new(
        new MemberResponse(result.MemberId, result.Name, result.Email),
        result.AccessToken,
        result.AccessTokenExpiresAt,
        result.RefreshToken,
        result.RefreshTokenExpiresAt);
}
