using DesafioCCAA.Application.UseCases.Books.ViewModels;
using DesafioCCAA.Shared.Responses;
using MediatR;

namespace DesafioCCAA.Application.UseCases.Books.Queries;

public record GetByIdUserCreatedBook(Guid Id) : IRequest<BaseResultList<BookViewModel>>;
