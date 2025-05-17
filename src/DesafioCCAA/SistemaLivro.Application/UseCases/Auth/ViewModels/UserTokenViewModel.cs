using System.Security.Claims;

namespace SistemaLivro.Application.UseCases.Auth.ViewModels;

public class UserTokenViewModel
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime BirthDate { get; set; }

    public IEnumerable<ClaimViewModel> Claims { get; set; }

    public UserTokenViewModel()
    {

    }

    public UserTokenViewModel(
        string id,
        string email,
        string name,
        DateTime birthDate,
        IList<Claim> claims)
    {
        Id = id;
        Email = email;
        Claims = claims.Select(c => new ClaimViewModel(c));
        Name = name;
        BirthDate = birthDate;
    }
}