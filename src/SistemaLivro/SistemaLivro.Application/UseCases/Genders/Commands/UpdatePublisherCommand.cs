using MediatR;
using SistemaLivro.Shared.Responses;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SistemaLivro.Application.UseCases.Genders.Commands;

public class UpdateGenderCommand : IRequest<BaseResult<Guid>>
{
    public Guid Id { get; set; }

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
