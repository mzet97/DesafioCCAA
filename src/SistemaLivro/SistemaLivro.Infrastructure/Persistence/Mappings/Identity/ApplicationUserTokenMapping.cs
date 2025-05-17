using SistemaLivro.Domain.Domains.Identities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SistemaLivro.Infrastructure.Persistence.Mappings.Identity;

public class ApplicationUserTokenMapping : IEntityTypeConfiguration<ApplicationUserToken>
{
    public void Configure(EntityTypeBuilder<ApplicationUserToken> builder)
    {
        builder.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });

        builder.ToTable("AspNetUserTokens");
    }
}
