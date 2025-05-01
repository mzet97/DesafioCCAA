using FluentValidation;

namespace DesafioCCAA.Shared.Models;

public abstract class ValueObject<T> : Validatable<T>
       where T : ValueObject<T>
{
    protected ValueObject(IValidator<T> validator)
        : base(validator)
    {
    }

    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object obj)
    {
        if (obj == null || obj.GetType() != typeof(T))
            return false;

        var other = (T)obj;
        using var thisValues = GetEqualityComponents().GetEnumerator();
        using var otherValues = other.GetEqualityComponents().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (!Equals(thisValues.Current, otherValues.Current))
                return false;
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return GetEqualityComponents()
                .Aggregate(17, (current, obj) => current * 23 + (obj?.GetHashCode() ?? 0));
        }
    }

    public static bool operator ==(ValueObject<T> a, ValueObject<T> b)
    {
        if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            return true;

        if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(ValueObject<T> a, ValueObject<T> b) => !(a == b);
}
