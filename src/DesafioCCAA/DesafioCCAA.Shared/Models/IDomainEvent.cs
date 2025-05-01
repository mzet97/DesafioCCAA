using MediatR;

namespace DesafioCCAA.Shared.Models;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
