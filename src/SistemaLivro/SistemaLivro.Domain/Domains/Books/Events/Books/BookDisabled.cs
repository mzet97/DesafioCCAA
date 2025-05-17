using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Books;

public class BookDisabled : DomainEvent
{
    public Book Book { get; }

    public BookDisabled(Book book)
    {
        Book = book;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
