using MediatR;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Shared.Responses;

namespace SistemaLivro.Application.UseCases.Books.Queries;

public record GetByIdUserCreatedBook(Guid Id) : IRequest<BaseResultList<BookViewModel>>;
