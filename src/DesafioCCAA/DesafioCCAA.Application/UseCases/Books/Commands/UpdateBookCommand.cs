using DesafioCCAA.Shared.Responses;
using MediatR;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DesafioCCAA.Application.UseCases.Books.Commands;

public class UpdateBookCommand : IRequest<BaseResult<Guid>>
{
    public Guid Id { get; set; }

    [DisplayName("Título")]
    [Required(ErrorMessage = "Informe a Título")]
    [MinLength(2)]
    [MaxLength(4000)]
    public string Title { get; set; }

    [DisplayName("Autor")]
    [Required(ErrorMessage = "Informe a Autor")]
    [MinLength(2)]
    [MaxLength(4000)]
    public string Author { get; set; }

    [DisplayName("Sinopse")]
    [Required(ErrorMessage = "Informe a Sinopse")]
    [MinLength(2)]
    [MaxLength(4000)]
    public string Synopsis { get; set; }

    [DisplayName("Número ISBN")]
    [Required(ErrorMessage = "Informe a Número ISBN")]
    [MinLength(2)]
    [MaxLength(4000)]
    public string ISBN { get; set; }

    [Required(ErrorMessage = "Informe a Gênero")]
    public Guid GenderId { get; set; }

    [Required(ErrorMessage = "Informe a Editora ")]
    public Guid PublisherId { get; set; }

    public Guid UserUpdatedId { get; set; }
}
