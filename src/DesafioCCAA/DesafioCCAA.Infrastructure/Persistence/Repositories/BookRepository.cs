using DesafioCCAA.Domain.Domains.Books;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DesafioCCAA.Infrastructure.Persistence.Repositories;

public class BookRepository : Repository<Book>, IBookRepository
{
    public BookRepository(ApplicationDbContext db) : base(db)
    {
    }

    public override async Task<IEnumerable<Book>> FindAsync(Expression<Func<Book, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return await DbSet
            .AsNoTracking()
            .Include("Gender")
            .Include("Publisher")
            .Where(predicate)
            .ToListAsync();
    }
}