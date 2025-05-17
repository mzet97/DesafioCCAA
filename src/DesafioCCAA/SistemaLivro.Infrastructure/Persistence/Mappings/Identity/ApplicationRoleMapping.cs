using SistemaLivro.Domain.Domains.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SistemaLivro.Infrastructure.Persistence.Mappings.Identity;

public class ApplicationRoleMapping : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(500)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.NormalizedName)
            .IsRequired()
            .HasMaxLength(500)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.ToTable("AspNetRoles");

        builder.HasMany(r => r.UserRoles)
            .WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        builder.HasMany(r => r.RoleClaims)
            .WithOne()
            .HasForeignKey(rc => rc.RoleId)
            .IsRequired();
    }
}
