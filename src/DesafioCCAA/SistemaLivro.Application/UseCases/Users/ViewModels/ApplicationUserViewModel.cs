using SistemaLivro.Domain.Domains.Identities;

namespace SistemaLivro.Application.UseCases.Users.ViewModels;

public class ApplicationUserViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }

    public ApplicationUserViewModel()
    {
        
    }

    public ApplicationUserViewModel(
        Guid id,
        string name,
        string email,
        DateTime birthDate)
    {
        Id = id;
        Name = name;
        Email = email;
        BirthDate = birthDate;
    }

    public static ApplicationUserViewModel FromEntity(ApplicationUser entity)
    {
        return new ApplicationUserViewModel(
            entity.Id,
            entity.UserName,
            entity.Email,
            entity.BirthDate);
    }
}
