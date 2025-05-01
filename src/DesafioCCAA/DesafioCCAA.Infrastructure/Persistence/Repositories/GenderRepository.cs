using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Domain.Repositories.Interfaces.Books;

namespace DesafioCCAA.Infrastructure.Persistence.Repositories;

public class GenderRepository : Repository<Gender>, IGenderRepository
{
    public GenderRepository(ApplicationDbContext db) : base(db)
    {
    }
}