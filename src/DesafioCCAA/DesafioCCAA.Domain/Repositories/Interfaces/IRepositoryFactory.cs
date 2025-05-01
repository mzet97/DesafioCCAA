using DesafioCCAA.Domain.Repositories.Interfaces.Books;

namespace DesafioCCAA.Domain.Repositories.Interfaces;

public interface IRepositoryFactory : IDisposable
{
    IBookRepository BookRepository { get; }
    IGenderRepository GenderRepository { get; }
    IPublisherRepository PublisherRepository { get; }

}
