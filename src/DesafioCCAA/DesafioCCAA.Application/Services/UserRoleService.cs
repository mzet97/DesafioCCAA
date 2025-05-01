using DesafioCCAA.Domain.Domains.Identities;
using DesafioCCAA.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DesafioCCAA.Application.Services;

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRoleService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task AddUserToRoleAsync(ApplicationUser user, string roleName)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

        var result = await _userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to add user to role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task RemoveUserFromRoleAsync(ApplicationUser user, string roleName)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));

        var result = await _userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to remove user from role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));
        return await _userManager.Users.Where(u => _userManager.IsInRoleAsync(u, roleName).Result).ToListAsync();
    }
}
