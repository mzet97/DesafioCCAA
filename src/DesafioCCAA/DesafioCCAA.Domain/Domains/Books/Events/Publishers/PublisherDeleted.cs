using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Shared.Models;
using System.Text.Json;

namespace DesafioCCAA.Domain.Domains.Books.Events.Publishers;

public class PublisherDeleted : DomainEvent
{
    public Publisher Publisher { get; }

    public PublisherDeleted(Publisher publisher)
    {
        Publisher = publisher;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
