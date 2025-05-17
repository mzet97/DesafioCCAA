using FluentValidation;
using System.Text.Json.Serialization;

namespace SistemaLivro.Shared.Models;

public abstract class Entity<T> : Validatable<T>, IEntity
        where T : Entity<T>
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    [JsonIgnore]
    protected List<IDomainEvent> _events;

    [JsonIgnore]
    public IEnumerable<IDomainEvent> Events => _events;

    protected Entity(IValidator<T> validator)
             : base(validator)
    {
        _errors = new List<string>();
        _isValid = false;
        _events = new List<IDomainEvent>();
    }

    protected Entity(
        IValidator<T> validator,
        Guid id,
        DateTime createdAt,
        DateTime? updatedAt,
        DateTime? deletedAt,
        bool isDeleted) : base(validator)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
        IsDeleted = isDeleted;
        _errors = new List<string>();
        _isValid = false;
        _events = new List<IDomainEvent>();
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity<T> left, Entity<T> right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (ReferenceEquals(left, null)
         || ReferenceEquals(right, null))
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(Entity<T> left, Entity<T> right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<T> entity &&
               Id.Equals(entity.Id) &&
               CreatedAt == entity.CreatedAt &&
               UpdatedAt == entity.UpdatedAt &&
               DeletedAt == entity.DeletedAt &&
               IsDeleted == entity.IsDeleted;
    }

    protected void AddEvent(IDomainEvent @event)
    {
        if (_events == null)
            _events = new List<IDomainEvent>();

        _events.Add(@event);
    }

    protected void ClearEvents() => _events.Clear();

    public virtual void Disabled()
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        Validate();
    }

    public virtual void Activate()
    {
        IsDeleted = false;
        DeletedAt = null;
        Validate();
    }

    public virtual void Update()
    {
        UpdatedAt = DateTime.Now;
        Validate();
    }
}