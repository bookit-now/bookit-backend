using BookIt.Application.Abstractions;
using BookIt.Domain.Members;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Infrastructure.Persistence;

internal sealed class MemberRepository(LibraryDbContext dbContext) : IMemberRepository
{
    public Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Members.SingleOrDefaultAsync(member => member.Id == id, cancellationToken);

    public Task<Member?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken) =>
        dbContext.Members.SingleOrDefaultAsync(
            member => member.NormalizedEmail == normalizedEmail,
            cancellationToken);

    public void Add(Member member) => dbContext.Members.Add(member);
}
