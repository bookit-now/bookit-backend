namespace BookIt.Application.Authentication;

public sealed class AuthenticationValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class EmailAlreadyRegisteredException()
    : Exception("A member with this email is already registered.");

public sealed class InvalidCredentialsException()
    : Exception("The supplied credentials are invalid.");

public sealed class InvalidRefreshTokenException()
    : Exception("The refresh token is invalid or expired.");
