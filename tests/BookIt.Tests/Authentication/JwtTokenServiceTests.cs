using BookIt.Application.Authentication;
using BookIt.Domain.Members;
using BookIt.Infrastructure.Authentication;

namespace BookIt.Tests.Authentication;

public sealed class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateSession_UsesConfiguredLifetimes()
    {
        var service = CreateService();
        var member = CreateMember();

        var tokens = service.CreateSession(member);

        Assert.Equal(Now.AddHours(1), tokens.AccessTokenExpiresAt);
        Assert.Equal(Now.AddDays(15), tokens.RefreshTokenExpiresAt);
    }

    [Fact]
    public void ValidateRefreshToken_AcceptsRefreshTokenAndRejectsAccessToken()
    {
        var service = CreateService();
        var member = CreateMember();
        var tokens = service.CreateSession(member);

        var payload = service.ValidateRefreshToken(tokens.RefreshToken);

        Assert.Equal(member.Id, payload.MemberId);
        Assert.Equal(tokens.RefreshTokenExpiresAt, payload.ExpiresAt);
        Assert.Throws<InvalidRefreshTokenException>(() =>
            service.ValidateRefreshToken(tokens.AccessToken));
    }

    private static JwtTokenService CreateService() => new(
        new JwtOptions
        {
            Issuer = "BookIt.Tests",
            AccessAudience = "bookit-api",
            RefreshAudience = "bookit-refresh",
            SigningKey = "bookit-tests-signing-key-with-more-than-32-bytes",
            AccessTokenMinutes = 60,
            RefreshTokenDays = 15
        },
        new FixedTimeProvider(Now));

    private static Member CreateMember() =>
        new("Ada Lovelace", "ada@example.com", "ADA@EXAMPLE.COM", "password-hash");

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
