using DesafioCCAA.Domain.Domains.Identities;
using Microsoft.AspNetCore.Identity;

namespace DesafioCCAA.Domain.Services.Interfaces;

public interface IApplicationUserService
{
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<ApplicationUser?> FindByIdAsync(Guid userId);
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    Task<IdentityResult> UpdateAsync(ApplicationUser user);
    Task<IdentityResult> DeleteAsync(Guid userId);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);

    Task<IList<ApplicationUserClaim>> GetClaimsAsync(ApplicationUser user);
    Task<IdentityResult> AddClaimToUserAsync(ApplicationUser user, string claimType, string claimValue);
    Task<IdentityResult> RemoveClaimFromUserAsync(ApplicationUser user, string claimType);

    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string roleName);
    Task<IdentityResult> RemoveUserFromRoleAsync(ApplicationUser user, string roleName);

    Task<IList<UserLoginInfo>> GetUserLoginsAsync(ApplicationUser user);
    Task<IdentityResult> AddLoginToUserAsync(ApplicationUser user, UserLoginInfo loginInfo);
    Task<IdentityResult> RemoveLoginFromUserAsync(ApplicationUser user, string loginProvider, string providerKey);

    Task<string> GenerateUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose);
    Task<bool> VerifyUserTokenAsync(ApplicationUser user, string tokenProvider, string purpose, string token);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);

    Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

    Task<SignInResult> PasswordSignInByEmailAsync(string email, string password, bool isPersistent, bool lockoutOnFailure);

    Task<SignInResult> CheckPasswordSignInAsync(ApplicationUser user, string password, bool lockoutOnFailure);
    Task RefreshSignInAsync(ApplicationUser user);

    Task TrySignInAsync(ApplicationUser user);

    Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
    Task<IdentityResult> ConfirmEmailAsync(ApplicationUser user, string token);
}
