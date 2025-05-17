using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using SistemaLivro.Infrastructure.Persistence;

namespace SistemaLivro.Infrastructure.Persistence.Repositories;

public class PublisherRepository : Repository<Publisher>, IPublisherRepository
{
    public PublisherRepository(ApplicationDbContext db) : base(db)
    {
    }
}