using SistemaLivro.Domain.Domains.Identities;

namespace SistemaLivro.Domain.Services.Interfaces;

public interface IUserClaimService
{
    Task AddClaimToUserAsync(ApplicationUser user, string claimType, string claimValue);

    Task RemoveClaimFromUserAsync(ApplicationUser user, string claimType);

    Task<IList<ApplicationUserClaim>> GetUserClaimsAsync(ApplicationUser user);
}
