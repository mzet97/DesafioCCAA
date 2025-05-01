using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DesafioCCAA.Application.Services;

public class UserClaimService : IUserClaimService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserClaimService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task AddClaimToUserAsync(ApplicationUser user, string claimType, string claimValue)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(claimType)) throw new ArgumentException("Claim type cannot be null or empty", nameof(claimType));
        if (string.IsNullOrEmpty(claimValue)) throw new ArgumentException("Claim value cannot be null or empty", nameof(claimValue));

        var claim = new Claim(claimType, claimValue);
        var result = await _userManager.AddClaimAsync(user, claim);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add claim to user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task RemoveClaimFromUserAsync(ApplicationUser user, string claimType)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(claimType)) throw new ArgumentException("Claim type cannot be null or empty", nameof(claimType));

        var claims = await _userManager.GetClaimsAsync(user);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
        if (claimToRemove != null)
        {
            var result = await _userManager.RemoveClaimAsync(user, claimToRemove);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to remove claim from user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            throw new InvalidOperationException("Claim not found in user");
        }
    }

    public async Task<IList<ApplicationUserClaim>> GetUserClaimsAsync(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Select(c => new ApplicationUserClaim { ClaimType = c.Type, ClaimValue = c.Value, UserId = user.Id }).ToList();
    }
}
