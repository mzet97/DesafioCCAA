using SistemaLivro.Domain.Domains.Identities;
using MediatR;
using SistemaLivro.Application.UseCases.Auth.ViewModels;
using SistemaLivro.Shared.Responses;
using System.ComponentModel.DataAnnotations;

namespace SistemaLivro.Application.UseCases.Auth.Commands;

public class RegisterUserCommand : IRequest<BaseResult<LoginResponseViewModel>>
{

    [Required(ErrorMessage = "O campo {0} é requerido")]
    [EmailAddress(ErrorMessage = "O campo {0} é inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O campo {0} é requerido")]
    [StringLength(255, ErrorMessage = "O campo  {0} deve está entre {2} e {1} caracteres", MinimumLength = 3)]
    public string Name { get; set; }

    [Required(ErrorMessage = "O campo {0} é requeridod")]
    [StringLength(255, ErrorMessage = "O campo  {0} deve está entre {2} e {1} caracteres", MinimumLength = 6)]
    public string Password { get; set; }

    [Required(ErrorMessage = "O campo {0} é requerido")]
    public DateTime BirthDate { get; set; }

    public RegisterUserCommand(string email, string name, string password, DateTime birthDate)
    {
        Email = email;
        Name = name;
        Password = password;
        BirthDate = birthDate;
    }

    public ApplicationUser ToDomain()
    {
        var entity = new ApplicationUser();

        entity.UserName = Name;
        entity.Email = Email;
        entity.NormalizedEmail = Email.ToUpper();
        entity.NormalizedUserName = Name.ToUpper();
        entity.BirthDate = BirthDate;

        return entity;
    }
}
