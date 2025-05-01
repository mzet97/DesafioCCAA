using DesafioCCAA.Domain.Domains.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioCCAA.Infrastructure.Persistence.Mappings.Identity;

public class ApplicationRoleClaimMapping : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        builder.HasKey(rc => rc.Id);

        builder.Property(u => u.ClaimType)
            .IsRequired()
            .HasMaxLength(500)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.ClaimValue)
            .IsRequired()
            .HasMaxLength(500)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.ToTable("AspNetRoleClaims");
    }
}
