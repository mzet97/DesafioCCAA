using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaLivro.Domain.Domains.Identities;

namespace SistemaLivro.Domain.Services.Interfaces;

public interface IUserRoleService
{
    Task AddUserToRoleAsync(ApplicationUser user, string roleName);

    Task RemoveUserFromRoleAsync(ApplicationUser user, string roleName);

    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);

    Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName);
}
