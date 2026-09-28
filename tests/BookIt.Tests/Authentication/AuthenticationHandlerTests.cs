using BookIt.Application.Abstractions;
using BookIt.Application.Authentication;
using BookIt.Domain.Members;

namespace BookIt.Tests.Authentication;

public sealed class AuthenticationHandlerTests
{
    private static readonly DateTimeOffset AccessExpiry = new(2026, 9, 27, 13, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset RefreshExpiry = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Register_NormalizesEmailAndReturnsTokens()
    {
        var repository = new FakeMemberRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterMemberHandler(
            repository,
            new FakePasswordService(),
            new FakeTokenService(),
            unitOfWork);

        var result = await handler.HandleAsync(
            new RegisterMemberCommand("Ada Lovelace", " Ada@Example.com ", "password123"),
            CancellationToken.None);

        var member = Assert.Single(repository.Members);
        Assert.Equal("ada@example.com", member.Email);
        Assert.Equal("ADA@EXAMPLE.COM", member.NormalizedEmail);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Register_RejectsShortPassword()
    {
        var handler = new RegisterMemberHandler(
            new FakeMemberRepository(),
            new FakePasswordService(),
            new FakeTokenService(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<AuthenticationValidationException>(() => handler.HandleAsync(
            new RegisterMemberCommand("Ada Lovelace", "ada@example.com", "short"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Register_RejectsDuplicateNormalizedEmail()
    {
        var repository = new FakeMemberRepository();
        repository.Add(new Member("Ada", "ada@example.com", "ADA@EXAMPLE.COM", "hash:password123"));
        var handler = new RegisterMemberHandler(
            repository,
            new FakePasswordService(),
            new FakeTokenService(),
            new FakeUnitOfWork());

        await Assert.ThrowsAsync<EmailAlreadyRegisteredException>(() => handler.HandleAsync(
            new RegisterMemberCommand("Another Ada", "ADA@example.com", "password123"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Login_RejectsIncorrectPassword()
    {
        var repository = new FakeMemberRepository();
        repository.Add(new Member("Ada", "ada@example.com", "ADA@EXAMPLE.COM", "hash:password123"));
        var handler = new LoginHandler(repository, new FakePasswordService(), new FakeTokenService());

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => handler.HandleAsync(
            new LoginCommand("ada@example.com", "incorrect-password"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Refresh_PreservesRefreshTokenAndOriginalExpiry()
    {
        var repository = new FakeMemberRepository();
        var member = new Member("Ada", "ada@example.com", "ADA@EXAMPLE.COM", "hash:password123");
        repository.Add(member);
        var tokens = new FakeTokenService(member.Id);
        var handler = new RefreshSessionHandler(repository, tokens);

        var result = await handler.HandleAsync(
            new RefreshSessionCommand("refresh-token"),
            CancellationToken.None);

        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal(RefreshExpiry, result.RefreshTokenExpiresAt);
        Assert.Equal("new-access-token", result.AccessToken);
    }

    private sealed class FakeMemberRepository : IMemberRepository
    {
        public List<Member> Members { get; } = [];

        public Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Members.SingleOrDefault(member => member.Id == id));

        public Task<Member?> GetByNormalizedEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken) =>
            Task.FromResult(Members.SingleOrDefault(member => member.NormalizedEmail == normalizedEmail));

        public void Add(Member member) => Members.Add(member);
    }

    private sealed class FakePasswordService : IPasswordService
    {
        public string Hash(string password) => $"hash:{password}";
        public bool Verify(string passwordHash, string password) => passwordHash == $"hash:{password}";
    }

    private sealed class FakeTokenService(Guid? refreshMemberId = null) : ITokenService
    {
        public TokenPair CreateSession(Member member) =>
            new("access-token", AccessExpiry, "refresh-token", RefreshExpiry);

        public AccessToken CreateAccessToken(Member member) => new("new-access-token", AccessExpiry);

        public RefreshTokenPayload ValidateRefreshToken(string refreshToken) =>
            refreshToken == "refresh-token" && refreshMemberId is Guid memberId
                ? new RefreshTokenPayload(memberId, RefreshExpiry)
                : throw new InvalidRefreshTokenException();
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }
}
