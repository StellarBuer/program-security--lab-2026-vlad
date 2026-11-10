namespace Game.Domain.Common;

/// <summary>
/// Базовий абстрактний клас для Value Objects (об'єктів-значень).
/// Реалізує порівняння за значенням компонентів замість порівняння за посиланням.
/// </summary>
public abstract class AValueObject : IEquatable<AValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        var other = (AValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(AValueObject? other)
    {
        if (other is null || other.GetType() != GetType())
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate(0, (hash, current) => HashCode.Combine(hash, current));
    }

    public static bool operator ==(AValueObject? left, AValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(AValueObject? left, AValueObject? right) => !(left == right);
}

