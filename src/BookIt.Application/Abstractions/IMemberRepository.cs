using BookIt.Domain.Members;

namespace BookIt.Application.Abstractions;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Member?> GetByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    void Add(Member member);
}
