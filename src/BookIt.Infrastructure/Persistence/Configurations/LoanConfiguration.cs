using BookIt.Domain.Books;
using BookIt.Domain.Loans;
using BookIt.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookIt.Infrastructure.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("loans", table =>
        {
            table.HasCheckConstraint("ck_loans_due_date", "due_date > borrowed_at");
            table.HasCheckConstraint(
                "ck_loans_returned_at",
                "returned_at IS NULL OR returned_at >= borrowed_at");
        });

        builder.HasKey(loan => loan.Id);
        builder.Property(loan => loan.Id).HasColumnName("id");
        builder.Property(loan => loan.BookId).HasColumnName("book_id");
        builder.Property(loan => loan.MemberId).HasColumnName("member_id");
        builder.Property(loan => loan.BorrowedAt).HasColumnName("borrowed_at");
        builder.Property(loan => loan.DueDate).HasColumnName("due_date");
        builder.Property(loan => loan.ReturnedAt).HasColumnName("returned_at");

        builder.HasOne<Book>()
            .WithMany()
            .HasForeignKey(loan => loan.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(loan => loan.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(loan => loan.BookId);
        builder.HasIndex(loan => loan.MemberId);
        builder.HasIndex(loan => loan.DueDate);
    }
}
