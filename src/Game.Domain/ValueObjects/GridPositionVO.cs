using Game.Domain.Common;
using Game.Domain.Exceptions;

namespace Game.Domain.ValueObjects;

public class GridPositionVO : AValueObject
{
    public int X { get; }
    public int Y { get; }

    public GridPositionVO(int x, int y)
    {
        // Загальне поле бою складається з двох блоків 5x5 (по ширині X від 0 до 9, по висоті Y від 0 до 4)
        if (x < 0 || x > 9)
            throw new DomainValidationException("Координата X має бути в межах від 0 до 9.");

        if (y < 0 || y > 4)
            throw new DomainValidationException("Координата Y має бути в межах від 0 до 4.");

        X = x;
        Y = y;
    }

    // Доменна політика: чи належить позиція зоні виставлення команд 3х3
    public bool IsValidForPlacement(bool isPlayerOne)
    {
        // Блок гравця: X від 1 до 3, Y від 1 до 3 (центр лівого блока 5x5)
        // Блок суперника: X від 6 до 8, Y від 1 до 3 (центр правого блока 5x5)
        if (isPlayerOne)
        {
            return X >= 1 && X <= 3 && Y >= 1 && Y <= 3;
        }
        else
        {
            return X >= 6 && X <= 8 && Y >= 1 && Y <= 3;
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}
