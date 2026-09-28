namespace BookIt.Domain.Loans;

public sealed class Loan
{
    private Loan()
    {
    }

    public Loan(Guid bookId, Guid memberId, DateTimeOffset borrowedAt, DateTimeOffset dueDate)
    {
        if (bookId == Guid.Empty)
        {
            throw new ArgumentException("A book is required.", nameof(bookId));
        }

        if (memberId == Guid.Empty)
        {
            throw new ArgumentException("A member is required.", nameof(memberId));
        }

        if (dueDate <= borrowedAt)
        {
            throw new ArgumentException("The due date must be after the borrowed date.", nameof(dueDate));
        }

        Id = Guid.NewGuid();
        BookId = bookId;
        MemberId = memberId;
        BorrowedAt = borrowedAt.ToUniversalTime();
        DueDate = dueDate.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public Guid MemberId { get; private set; }
    public DateTimeOffset BorrowedAt { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public DateTimeOffset? ReturnedAt { get; private set; }

    public void MarkReturned(DateTimeOffset returnedAt)
    {
        if (ReturnedAt is not null)
        {
            throw new InvalidOperationException("This loan has already been returned.");
        }

        if (returnedAt < BorrowedAt)
        {
            throw new ArgumentException("The return date cannot be before the borrowed date.", nameof(returnedAt));
        }

        ReturnedAt = returnedAt.ToUniversalTime();
    }
}
