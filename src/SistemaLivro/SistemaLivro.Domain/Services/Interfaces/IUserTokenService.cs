using SistemaLivro.Domain.Domains.Identities;

namespace SistemaLivro.Domain.Services.Interfaces;

public interface IUserTokenService
{
    Task AddUserTokenAsync(ApplicationUserToken userToken);

    Task<ApplicationUserToken?> GetUserTokenAsync(Guid userId, string loginProvider, string name);

    Task RemoveUserTokenAsync(ApplicationUserToken userToken);
}