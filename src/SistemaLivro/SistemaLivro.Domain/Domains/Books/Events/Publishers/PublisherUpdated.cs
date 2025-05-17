using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Publishers;

public class PublisherUpdated : DomainEvent
{
    public Publisher Publisher { get; }

    public PublisherUpdated(Publisher publisher)
    {
        Publisher = publisher;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
