using DesafioCCAA.Domain.Domains.Books.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioCCAA.Infrastructure.Persistence.Mappings;

public class PublisherMapping : IEntityTypeConfiguration<Publisher>
{
    public void Configure(EntityTypeBuilder<Publisher> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired(true)
            .HasColumnType("varchar")
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .IsRequired(false)
            .HasColumnType("varchar")
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DeletedAt);

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.ToTable("Publisher");
    }
}
