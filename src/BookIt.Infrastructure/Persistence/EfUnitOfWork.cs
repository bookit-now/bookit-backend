using BookIt.Application.Abstractions;
using BookIt.Application.Authentication;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookIt.Infrastructure.Persistence;

internal sealed class EfUnitOfWork(LibraryDbContext dbContext) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "IX_members_normalized_email"
            })
        {
            throw new EmailAlreadyRegisteredException();
        }
    }
}
