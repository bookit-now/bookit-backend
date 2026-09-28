using BookIt.Domain.Loans;

namespace BookIt.Tests.Domain;

public sealed class LoanTests
{
    [Fact]
    public void Constructor_RejectsDueDateBeforeBorrowDate()
    {
        var borrowedAt = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new Loan(Guid.NewGuid(), Guid.NewGuid(), borrowedAt, borrowedAt));
    }

    [Fact]
    public void MarkReturned_CannotReturnTwice()
    {
        var borrowedAt = DateTimeOffset.UtcNow;
        var loan = new Loan(Guid.NewGuid(), Guid.NewGuid(), borrowedAt, borrowedAt.AddDays(14));

        loan.MarkReturned(borrowedAt.AddDays(1));

        Assert.Throws<InvalidOperationException>(() => loan.MarkReturned(borrowedAt.AddDays(2)));
    }
}
