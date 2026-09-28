using BookIt.Application.Abstractions;
using BookIt.Domain.Members;

namespace BookIt.Application.Authentication;

public sealed class RegisterMemberHandler(
    IMemberRepository members,
    IPasswordService passwords,
    ITokenService tokens,
    IUnitOfWork unitOfWork)
{
    public async Task<AuthenticationResult> HandleAsync(
        RegisterMemberCommand command,
        CancellationToken cancellationToken)
    {
        AuthenticationHelpers.ValidateRegistration(command.Name, command.Email, command.Password);

        var normalizedEmail = AuthenticationHelpers.NormalizeEmail(command.Email);
        if (await members.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken) is not null)
        {
            throw new EmailAlreadyRegisteredException();
        }

        var member = new Member(
            command.Name,
            command.Email.Trim().ToLowerInvariant(),
            normalizedEmail,
            passwords.Hash(command.Password));

        members.Add(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResult(member, tokens.CreateSession(member));
    }

    internal static AuthenticationResult ToResult(Member member, TokenPair tokenPair) => new(
        member.Id,
        member.Name,
        member.Email,
        tokenPair.AccessToken,
        tokenPair.AccessTokenExpiresAt,
        tokenPair.RefreshToken,
        tokenPair.RefreshTokenExpiresAt);
}
