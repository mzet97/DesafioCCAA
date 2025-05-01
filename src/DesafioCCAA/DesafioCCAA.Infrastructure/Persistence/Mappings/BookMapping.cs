using DesafioCCAA.Domain.Domains.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioCCAA.Infrastructure.Persistence.Mappings;

public class BookMapping : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .HasMaxLength(150)
            .IsRequired()
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(o => o.Author)
            .HasMaxLength(150)
            .IsRequired()
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(o => o.Synopsis)
            .HasMaxLength(4000)
            .IsRequired()
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(o => o.ISBN)
            .HasMaxLength(13)
            .IsRequired()
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt);

        builder.Property(o => o.DeletedAt);

        builder.Property(o => o.IsDeleted)
            .IsRequired();

        builder.Property(o => o.GenderId)
            .IsRequired();

        builder.HasOne(o => o.Gender)
            .WithMany()
            .HasForeignKey(o => o.GenderId);

        builder.Property(o => o.PublisherId)
            .IsRequired();

        builder.HasOne(o => o.Publisher)
            .WithMany()
            .HasForeignKey(o => o.PublisherId);

        builder.HasOne(o => o.UserCreated)
            .WithMany()
            .HasForeignKey(o => o.UserCreatedId)
            .IsRequired()
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.UserUpdated)
            .WithMany()
            .HasForeignKey(o => o.UserUpdatedId)
            .IsRequired()
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable("Book");
    }
}
