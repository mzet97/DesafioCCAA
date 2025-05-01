using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;

namespace DesafioCCAA.Infrastructure.Persistence.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext db) : base(db)
    {
    }
}