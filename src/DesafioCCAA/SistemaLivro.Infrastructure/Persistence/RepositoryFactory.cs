using SistemaLivro.Domain.Repositories.Interfaces;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using SistemaLivro.Infrastructure.Persistence.Repositories;

namespace SistemaLivro.Infrastructure.Persistence;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly ApplicationDbContext _dbContext;

    public RepositoryFactory(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    private IBookRepository _bookRepository;
    public IBookRepository BookRepository { get => _bookRepository ?? (_bookRepository = new BookRepository(_dbContext)); }

    private IGenderRepository _genderRepository;
    public IGenderRepository GenderRepository { get => _genderRepository ?? (_genderRepository = new GenderRepository(_dbContext)); }

    private IPublisherRepository _publisherRepository;
    public IPublisherRepository PublisherRepository { get => _publisherRepository ?? (_publisherRepository = new PublisherRepository(_dbContext)); }
}
