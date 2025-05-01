using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DesafioCCAA.Application.Services;

public class ApplicationUserService : IApplicationUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ApplicationUserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.CreateAsync(user, password);
    }

    public async Task<ApplicationUser?> FindByIdAsync(Guid userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        if (string.IsNullOrEmpty(email)) throw new ArgumentException("Email cannot be null or empty", nameof(email));
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
    {
        return await _userManager.Users.ToListAsync();
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.UpdateAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "User not found" });
        }
        return await _userManager.DeleteAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IList<ApplicationUserClaim>> GetClaimsAsync(ApplicationUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        return claims.Select(c => new ApplicationUserClaim { ClaimType = c.Type, ClaimValue = c.Value, UserId = user.Id }).ToList();
    }

    public async Task<IdentityResult> AddClaimToUserAsync(ApplicationUser user, string claimType, string claimValue)
    {
        return await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claimType, claimValue));
    }

    public async Task<IdentityResult> RemoveClaimFromUserAsync(ApplicationUser user, string claimType)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
        if (claimToRemove != null)
        {
            return await _userManager.RemoveClaimAsync(user, claimToRemove);
        }
        return IdentityResult.Failed(new IdentityError { Description = "Claim not found" });
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName)
    {
        return await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> RemoveUserFromRoleAsync(ApplicationUser user, string roleName)
    {
        return await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task<IList<UserLoginInfo>> GetUserLoginsAsync(ApplicationUser user)
    {
        return await _userManager.GetLoginsAsync(user);
    }

    public async Task<IdentityResult> AddLoginToUserAsync(ApplicationUser user, UserLoginInfo loginInfo)
    {
        return await _userManager.AddLoginAsync(user, loginInfo);
    }

    public async Task<IdentityResult> RemoveLoginFromUserAsync(ApplicationUser user, string loginProvider, string providerKey)
    {
        return await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
    }

    public async Task<string> GenerateUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose)
    {
        return await _userManager.GenerateUserTokenAsync(user, tokenProvider, purpose);
    }

    public async Task<bool> VerifyUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose, string token)
    {
        return await _userManager.VerifyUserTokenAsync(user, tokenProvider, purpose, token);
    }

    public async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
    {
        return await _signInManager.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
    }

    public async Task TrySignInAsync(ApplicationUser user)
    {
        await _signInManager.SignInAsync(user, false);
    }

    public async Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure)
    {
        return await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure);
    }

    public async Task RefreshSignInAsync(ApplicationUser user)
    {
        await _signInManager.RefreshSignInAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.ConfirmEmailAsync(user, token);
    }
}