using SistemaLivro.Domain.Domains.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SistemaLivro.Infrastructure.Persistence.Mappings.Identity;

public class ApplicationUserLoginMapping : IEntityTypeConfiguration<ApplicationUserLogin>
{
    public void Configure(EntityTypeBuilder<ApplicationUserLogin> builder)
    {
        builder.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });

        builder.ToTable("AspNetUserLogins");
    }
}
