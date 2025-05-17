using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Publishers.Commands;

public class CreatePublisherCommand : IRequest<BaseResult<Guid>>
{
    [DisplayName("Nome")]
    [Required(ErrorMessage = "Informe o Nome")]
    [MinLength(2)]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [DisplayName("Descrição")]
    [Required(ErrorMessage = "Informe a Descrição")]
    [MinLength(2)]
    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;
}
