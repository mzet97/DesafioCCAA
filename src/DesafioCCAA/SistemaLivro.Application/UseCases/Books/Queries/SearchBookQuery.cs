using MediatR;
using SistemaLivro.Application.UseCases.Books.ViewModels;
using SistemaLivro.Shared.Responses;
using SistemaLivro.Shared.ViewModels;

namespace SistemaLivro.Application.UseCases.Books.Queries;

public class SearchBookQuery : BaseSearch, IRequest<BaseResultList<BookViewModel>>
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Synopsis { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string GenderName { get; set; } = string.Empty;
    public string PublisherName { get; set; } = string.Empty;
}
