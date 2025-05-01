using DesafioCCAA.Shared.Models;
using System.Text.Json;

namespace DesafioCCAA.Domain.Domains.Books.Events.Books;

public class BookUpdated : DomainEvent
{
    public Book Book { get; set; }

    public BookUpdated(Book book)
    {
        Book = book;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
