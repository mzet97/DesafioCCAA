using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Publishers;

public class PublisherDisabled : DomainEvent
{
    public Publisher Publisher { get; }

    public PublisherDisabled(Publisher publisher)
    {
        Publisher = publisher;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
