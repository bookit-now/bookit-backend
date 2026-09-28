using BookIt.Application.Authentication;

namespace BookIt.Application;

public interface ILibraryFacade
{
    Task<AuthenticationResult> RegisterMemberAsync(RegisterMemberCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult> LoginAsync(LoginCommand command, CancellationToken cancellationToken);
    Task<AuthenticationResult> RefreshSessionAsync(RefreshSessionCommand command, CancellationToken cancellationToken);
}
