using DesafioCCAA.Domain.Repositories.Interfaces;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using DesafioCCAA.Infrastructure.Persistence.Repositories;

namespace DesafioCCAA.Infrastructure.Persistence;

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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private bool _disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _bookRepository?.Dispose();
            _genderRepository?.Dispose();
            _publisherRepository?.Dispose();
        }

        _disposed = true;
    }

    ~RepositoryFactory()
    {
        Dispose(false);
    }
}
