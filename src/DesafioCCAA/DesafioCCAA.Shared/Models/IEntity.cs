namespace DesafioCCAA.Shared.Models;

public interface IEntity
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    DateTime? DeletedAt { get; }
    bool IsDeleted { get; }
}