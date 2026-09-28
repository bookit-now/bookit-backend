using BookIt.Domain.Books;

namespace BookIt.Tests.Domain;

public sealed class BookTests
{
    [Fact]
    public void BorrowAndReturnCopy_KeepAvailabilityWithinTotalCopies()
    {
        var book = new Book("Domain-Driven Design", "Eric Evans", "9780321125217", 1);

        book.BorrowCopy();
        Assert.Equal(0, book.AvailableCopies);
        Assert.Throws<InvalidOperationException>(book.BorrowCopy);

        book.ReturnCopy();
        Assert.Equal(1, book.AvailableCopies);
        Assert.Throws<InvalidOperationException>(book.ReturnCopy);
    }

    [Fact]
    public void Constructor_RejectsNonPositiveCopyCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Book("A Book", "An Author", "1234567890", 0));
    }
}
