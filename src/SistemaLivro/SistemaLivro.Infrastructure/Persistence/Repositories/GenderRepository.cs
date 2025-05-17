using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Domain.Repositories.Interfaces.Books;
using SistemaLivro.Infrastructure.Persistence;

namespace SistemaLivro.Infrastructure.Persistence.Repositories;

public class GenderRepository : Repository<Gender>, IGenderRepository
{
    public GenderRepository(ApplicationDbContext db) : base(db)
    {
    }
}