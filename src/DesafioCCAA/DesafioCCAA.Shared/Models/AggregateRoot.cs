using FluentValidation;

namespace DesafioCCAA.Shared.Models;

public abstract class AggregateRoot<T> : Entity<T>
        where T : Entity<T>
{
    protected AggregateRoot(IValidator<T> validator)
        : base(validator)
    {
    }

    protected AggregateRoot(
        IValidator<T> validator,
        Guid id,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        bool isDeleted)
        : base(validator, id, createdAt, updatedAt, deletedAt, isDeleted)
    {
    }
}
