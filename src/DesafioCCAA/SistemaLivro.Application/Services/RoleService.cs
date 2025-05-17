using SistemaLivro.Domain.Domains.Identities;
using SistemaLivro.Domain.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SistemaLivro.Application.Services;

public class RoleService : IRoleService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
    }

    public async Task<IdentityResult> CreateRoleAsync(ApplicationRole role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        return await _roleManager.CreateAsync(role);
    }

    public async Task<ApplicationRole?> GetRoleByIdAsync(Guid roleId)
    {
        return await _roleManager.FindByIdAsync(roleId.ToString());
    }

    public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));
        return await _roleManager.FindByNameAsync(roleName);
    }

    public async Task<IEnumerable<ApplicationRole>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    public async Task<IdentityResult> UpdateRoleAsync(ApplicationRole role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        return await _roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(Guid roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId.ToString());
        if (role == null)
        {
            return IdentityResult.Failed(new IdentityError { Description = "Role not found" });
        }
        return await _roleManager.DeleteAsync(role);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        if (string.IsNullOrEmpty(roleName)) throw new ArgumentException("Role name cannot be null or empty", nameof(roleName));
        return await _roleManager.RoleExistsAsync(roleName);
    }
}

