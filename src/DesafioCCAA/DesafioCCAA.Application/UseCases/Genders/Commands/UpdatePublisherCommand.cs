using DesafioCCAA.Shared.Responses;
using MediatR;
using System.ComponentModel;

namespace DesafioCCAA.Application.UseCases.Genders.Commands;

public class UpdateGenderCommand : IRequest<BaseResult<Guid>>
{
    public Guid Id { get; set; }
    [DisplayName("Nome")]
    public string Name { get; set; } = string.Empty;
    [DisplayName("Descrição")]
    public string Description { get; set; } = string.Empty;
}
