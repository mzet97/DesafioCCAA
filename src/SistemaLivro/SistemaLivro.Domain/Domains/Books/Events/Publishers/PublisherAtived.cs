using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Publishers;

public class PublisherAtived : DomainEvent
{
    public Publisher Publisher { get; set; }

    public PublisherAtived(Publisher publisher)
    {
        Publisher = publisher;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}