using SistemaLivro.Domain.Repositories.Interfaces.Books;

namespace SistemaLivro.Domain.Repositories.Interfaces;

public interface IRepositoryFactory
{
    IBookRepository BookRepository { get; }
    IGenderRepository GenderRepository { get; }
    IPublisherRepository PublisherRepository { get; }

}
