using SistemaLivro.Domain.Domains.Books.Entities;
using SistemaLivro.Shared.Models;
using System.Text.Json;

namespace SistemaLivro.Domain.Domains.Books.Events.Genders;

public class GenderDisabled : DomainEvent
{
    public Gender Gender { get; }

    public GenderDisabled(Gender gender)
    {
        Gender = gender;
    }

    public override string? ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
