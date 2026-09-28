using BookIt.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookIt.Infrastructure.Persistence.Configurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books", table =>
        {
            table.HasCheckConstraint("ck_books_total_copies", "total_copies > 0");
            table.HasCheckConstraint(
                "ck_books_available_copies",
                "available_copies >= 0 AND available_copies <= total_copies");
        });

        builder.HasKey(book => book.Id);
        builder.Property(book => book.Id).HasColumnName("id");
        builder.Property(book => book.Title).HasColumnName("title").HasMaxLength(300).IsRequired();
        builder.Property(book => book.Author).HasColumnName("author").HasMaxLength(200).IsRequired();
        builder.Property(book => book.Isbn).HasColumnName("isbn").HasMaxLength(20).IsRequired();
        builder.Property(book => book.TotalCopies).HasColumnName("total_copies");
        builder.Property(book => book.AvailableCopies).HasColumnName("available_copies");

        builder.HasIndex(book => book.Isbn).IsUnique();
        builder.HasIndex(book => book.Title);
        builder.HasIndex(book => book.Author);
    }
}
