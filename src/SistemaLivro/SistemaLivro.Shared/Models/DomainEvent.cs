namespace SistemaLivro.Shared.Models;


public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; protected set; }

    protected DomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}