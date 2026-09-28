using BookIt.Domain.Members;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookIt.Infrastructure.Persistence.Configurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");

        builder.HasKey(member => member.Id);
        builder.Property(member => member.Id).HasColumnName("id");
        builder.Property(member => member.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(member => member.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(member => member.NormalizedEmail)
            .HasColumnName("normalized_email")
            .HasMaxLength(320)
            .IsRequired();
        builder.Property(member => member.PasswordHash).HasColumnName("password_hash").IsRequired();

        builder.HasIndex(member => member.NormalizedEmail).IsUnique();
    }
}
