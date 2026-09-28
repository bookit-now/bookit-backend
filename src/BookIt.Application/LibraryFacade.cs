using BookIt.Application.Authentication;

namespace BookIt.Application;

public sealed class LibraryFacade(
    RegisterMemberHandler registerMember,
    LoginHandler login,
    RefreshSessionHandler refreshSession) : ILibraryFacade
{
    public Task<AuthenticationResult> RegisterMemberAsync(
        RegisterMemberCommand command,
        CancellationToken cancellationToken) => registerMember.HandleAsync(command, cancellationToken);

    public Task<AuthenticationResult> LoginAsync(
        LoginCommand command,
        CancellationToken cancellationToken) => login.HandleAsync(command, cancellationToken);

    public Task<AuthenticationResult> RefreshSessionAsync(
        RefreshSessionCommand command,
        CancellationToken cancellationToken) => refreshSession.HandleAsync(command, cancellationToken);
}
