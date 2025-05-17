using SistemaLivro.Domain.Domains.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SistemaLivro.Infrastructure.Persistence.Mappings.Identity;

public class ApplicationUserClaimMapping : IEntityTypeConfiguration<ApplicationUserClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationUserClaim> builder)
    {
        builder.HasKey(uc => uc.Id);

        builder.Property(u => u.ClaimValue)
           .IsRequired()
           .HasMaxLength(500)
           .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.ClaimType)
           .IsRequired()
           .HasMaxLength(500)
           .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.ToTable("AspNetUserClaims");
    }
}
