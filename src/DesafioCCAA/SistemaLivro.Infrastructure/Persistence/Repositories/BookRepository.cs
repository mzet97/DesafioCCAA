using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using Microsoft.EntityFrameworkCore;
using SistemaLivro.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace SistemaLivro.Infrastructure.Persistence.Repositories;

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