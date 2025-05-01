using DesafioCCAA.Domain.Domains.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DesafioCCAA.Infrastructure.Persistence.Mappings.Identity;

public class ApplicationUserMapping : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(256)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.NormalizedUserName)
            .HasMaxLength(256)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.NormalizedEmail)
            .HasMaxLength(256)
            .UseCollation("SQL_Latin1_General_CP1_CI_AS");

        builder.Property(u => u.BirthDate)
            .IsRequired();

        builder.HasIndex(u => u.NormalizedUserName)
            .IsUnique()
            .HasName("UserNameIndex");

        builder.HasIndex(u => u.NormalizedEmail)
            .HasName("EmailIndex");

        builder.HasMany(u => u.Claims)
            .WithOne()
            .HasForeignKey(uc => uc.UserId)
            .IsRequired();

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

        builder.HasMany(u => u.Logins)
            .WithOne()
            .HasForeignKey(ul => ul.UserId)
            .IsRequired();

        builder.HasMany(u => u.Tokens)
            .WithOne()
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        builder.ToTable("AspNetUsers");
    }
}
