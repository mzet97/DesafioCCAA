using SistemaLivro.Domain.Domains.Books;
using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Books;

public class BookCreated : DomainEvent
{
    public Book Book { get; set; }

    public BookCreated(Book book)
    {
        Book = book;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
