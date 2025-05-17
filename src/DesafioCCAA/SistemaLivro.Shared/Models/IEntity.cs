namespace SistemaLivro.Shared.Models;

public interface IEntity
{
    Guid Id { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
    DateTime? DeletedAt { get; }
    bool IsDeleted { get; }
}