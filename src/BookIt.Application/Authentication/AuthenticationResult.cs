namespace BookIt.Application.Authentication;

public sealed record AuthenticationResult(
    Guid MemberId,
    string Name,
    string Email,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
