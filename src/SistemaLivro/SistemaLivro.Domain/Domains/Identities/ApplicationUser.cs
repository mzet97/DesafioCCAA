using Microsoft.AspNetCore.Identity;

namespace SistemaLivro.Domain.Domains.Identities;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateTime BirthDate { get; set; }

    public ICollection<ApplicationUserClaim> Claims { get; set; } = null!;
    public ICollection<ApplicationUserRole> UserRoles { get; set; } = null!;
    public ICollection<ApplicationUserLogin> Logins { get; set; } = null!;
    public ICollection<ApplicationUserToken> Tokens { get; set; } = null!;
}
