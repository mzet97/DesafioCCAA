using DesafioCCAA.Shared.Responses;
using MediatR;
using System.ComponentModel;

namespace DesafioCCAA.Application.UseCases.Genders.Commands;

public class CreateGenderCommand : IRequest<BaseResult<Guid>>
{
    [DisplayName("Nome")]
    public string Name { get; set; } = string.Empty;
    [DisplayName("Descrição")]
    public string Description { get; set; } = string.Empty;
}
