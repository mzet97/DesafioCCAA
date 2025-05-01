using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Shared.Models;
using System.Text.Json;

namespace DesafioCCAA.Domain.Domains.Books.Events.Genders;

public class GenderDeleted : DomainEvent
{
    public Gender Gender { get; }

    public GenderDeleted(Gender gender)
    {
        Gender = gender;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
