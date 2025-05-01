using DesafioCCAA.Domain.Domains.Books.Entities;
using DesafioCCAA.Shared.Models;
using System.Text.Json;

namespace DesafioCCAA.Domain.Domains.Books.Events.Genders;

public class GenderCreated : DomainEvent
{
    public Gender Gender { get; set; }

    public GenderCreated(Gender gender)
    {
        Gender = gender;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
