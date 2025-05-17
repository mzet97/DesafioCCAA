using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Books;

public class BookAtived : DomainEvent
{
    public Book Book { get; set; }

    public BookAtived(Book book)
    {
        Book = book;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}