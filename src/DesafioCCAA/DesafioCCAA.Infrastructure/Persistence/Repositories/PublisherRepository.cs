using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;

namespace DesafioCCAA.Infrastructure.Persistence.Repositories;

public class PublisherRepository : Repository<Publisher>, IPublisherRepository
{
    public PublisherRepository(ApplicationDbContext db) : base(db)
    {
    }
}