using BookIt.Application.Abstractions;

namespace BookIt.Application.Authentication;

public sealed class RefreshSessionHandler(IMemberRepository members, ITokenService tokens)
{
    public async Task<AuthenticationResult> HandleAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken))
        {
            throw new InvalidRefreshTokenException();
        }

        var payload = tokens.ValidateRefreshToken(command.RefreshToken);
        var member = await members.GetByIdAsync(payload.MemberId, cancellationToken)
            ?? throw new InvalidRefreshTokenException();
        var accessToken = tokens.CreateAccessToken(member);

        return new AuthenticationResult(
            member.Id,
            member.Name,
            member.Email,
            accessToken.Value,
            accessToken.ExpiresAt,
            command.RefreshToken,
            payload.ExpiresAt);
    }
}
