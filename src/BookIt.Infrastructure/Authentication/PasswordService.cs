using BookIt.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace BookIt.Infrastructure.Authentication;

internal sealed class PasswordService(IPasswordHasher<object> passwordHasher) : IPasswordService
{
    private static readonly object PasswordOwner = new();

    public string Hash(string password) => passwordHasher.HashPassword(PasswordOwner, password);

    public bool Verify(string passwordHash, string password) =>
        passwordHasher.VerifyHashedPassword(PasswordOwner, passwordHash, password)
        is not PasswordVerificationResult.Failed;
}
