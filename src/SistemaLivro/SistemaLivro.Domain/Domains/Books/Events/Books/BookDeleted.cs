using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Books;

public class BookDeleted : DomainEvent
{
    public Book Book { get; }

    public BookDeleted(Book book)
    {
        Book = book;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
