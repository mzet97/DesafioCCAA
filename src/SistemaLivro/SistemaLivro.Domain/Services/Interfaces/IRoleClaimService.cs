using SistemaLivro.Domain.Domains.Identities;

namespace SistemaLivro.Domain.Services.Interfaces;

public interface IRoleClaimService
{
    Task AddClaimToRoleAsync(ApplicationRole role, string claimType, string claimValue);

    Task RemoveClaimFromRoleAsync(ApplicationRole role, string claimType);

    Task<IList<ApplicationRoleClaim>> GetRoleClaimsAsync(ApplicationRole role);
}
