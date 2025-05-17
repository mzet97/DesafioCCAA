using MediatR;

namespace SistemaLivro.Shared.Models;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
