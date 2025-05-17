using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace SistemaLivro.Application.Services;

public class RoleClaimService : IRoleClaimService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleClaimService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    public async Task AddClaimToRoleAsync(ApplicationRole role, string claimType, string claimValue)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (string.IsNullOrEmpty(claimType)) throw new ArgumentException("Claim type cannot be null or empty", nameof(claimType));
        if (string.IsNullOrEmpty(claimValue)) throw new ArgumentException("Claim value cannot be null or empty", nameof(claimValue));

        var claim = new Claim(claimType, claimValue);
        var result = await _roleManager.AddClaimAsync(role, claim);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add claim to role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task RemoveClaimFromRoleAsync(ApplicationRole role, string claimType)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (string.IsNullOrEmpty(claimType)) throw new ArgumentException("Claim type cannot be null or empty", nameof(claimType));

        var claims = await _roleManager.GetClaimsAsync(role);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
        if (claimToRemove != null)
        {
            var result = await _roleManager.RemoveClaimAsync(role, claimToRemove);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to remove claim from role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            throw new InvalidOperationException("Claim not found in role");
        }
    }

    public async Task<IList<ApplicationRoleClaim>> GetRoleClaimsAsync(ApplicationRole role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        var claims = await _roleManager.GetClaimsAsync(role);
        return claims.Select(c => new ApplicationRoleClaim { ClaimType = c.Type, ClaimValue = c.Value, RoleId = role.Id }).ToList();
    }
}
