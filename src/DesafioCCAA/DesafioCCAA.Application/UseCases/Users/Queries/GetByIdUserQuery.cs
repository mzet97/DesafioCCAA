using DesafioCCAA.Application.UseCases.Users.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Users.Queries;

public record GetByIdUserQuery(Guid Id) : IRequest<BaseResult<ApplicationUserViewModel>>;
