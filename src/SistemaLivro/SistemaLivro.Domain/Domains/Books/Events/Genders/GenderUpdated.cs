using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Genders;

public class GenderUpdated : DomainEvent
{

    public Gender Gender { get; set; }

    public GenderUpdated(Gender gender)
    {
        Gender = gender;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
