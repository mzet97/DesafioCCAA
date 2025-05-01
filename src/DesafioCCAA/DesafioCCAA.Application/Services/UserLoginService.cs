using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace DesafioCCAA.Application.Services;

public class UserLoginService : IUserLoginService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserLoginService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (login == null) throw new ArgumentNullException(nameof(login));

        var result = await _userManager.AddLoginAsync(user, login);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add login to user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(loginProvider)) throw new ArgumentException("Login provider cannot be null or empty", nameof(loginProvider));
        if (string.IsNullOrEmpty(providerKey)) throw new ArgumentException("Provider key cannot be null or empty", nameof(providerKey));

        var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to remove login from user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.GetLoginsAsync(user);
    }

    public async Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey)
    {
        if (string.IsNullOrEmpty(loginProvider)) throw new ArgumentException("Login provider cannot be null or empty", nameof(loginProvider));
        if (string.IsNullOrEmpty(providerKey)) throw new ArgumentException("Provider key cannot be null or empty", nameof(providerKey));

        return await _userManager.FindByLoginAsync(loginProvider, providerKey);
    }
}
