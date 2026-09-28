namespace BookIt.Domain.Books;

public sealed class Book
{
    private Book()
    {
    }

    public Book(string title, string author, string isbn, int totalCopies)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(author);
        ArgumentException.ThrowIfNullOrWhiteSpace(isbn);

        if (totalCopies <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCopies), "A book must have at least one copy.");
        }

        Id = Guid.NewGuid();
        Title = title.Trim();
        Author = author.Trim();
        Isbn = isbn.Trim();
        TotalCopies = totalCopies;
        AvailableCopies = totalCopies;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string Isbn { get; private set; } = string.Empty;
    public int TotalCopies { get; private set; }
    public int AvailableCopies { get; private set; }

    public void BorrowCopy()
    {
        if (AvailableCopies == 0)
        {
            throw new InvalidOperationException("No copies are available.");
        }

        AvailableCopies--;
    }

    public void ReturnCopy()
    {
        if (AvailableCopies >= TotalCopies)
        {
            throw new InvalidOperationException("All copies are already available.");
        }

        AvailableCopies++;
    }
}
