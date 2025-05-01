using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesafioCCAA.Domain.Domains.Identities;
using Microsoft.AspNetCore.Identity;

namespace DesafioCCAA.Domain.Services.Interfaces;

public interface IUserLoginService
{
    Task AddLoginAsync(ApplicationUser user, UserLoginInfo login);

    Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey);

    Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);

    Task<ApplicationUser?> FindByLoginAsync(string loginProvider, string providerKey);
}
