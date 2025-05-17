using MediatR;
using SistemaLivro.Application.UseCases.Users.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Users.Queries;

public record GetByIdUserQuery(Guid Id) : IRequest<BaseResult<ApplicationUserViewModel>>;
