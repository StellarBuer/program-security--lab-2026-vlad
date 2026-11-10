using Game.Domain.Entities;

namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс сервісу генерації JWT-токенів автентифікації.
/// </summary>
public interface ITokenService
{
    string GenerateToken(UserAccount user);
}

