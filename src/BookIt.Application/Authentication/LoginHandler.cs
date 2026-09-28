using BookIt.Application.Abstractions;

namespace BookIt.Application.Authentication;

public sealed class LoginHandler(
    IMemberRepository members,
    IPasswordService passwords,
    ITokenService tokens)
{
    public async Task<AuthenticationResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        AuthenticationHelpers.ValidateLogin(command.Email, command.Password);

        var member = await members.GetByNormalizedEmailAsync(
            AuthenticationHelpers.NormalizeEmail(command.Email),
            cancellationToken);

        if (member is null || !passwords.Verify(member.PasswordHash, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        return RegisterMemberHandler.ToResult(member, tokens.CreateSession(member));
    }
}
