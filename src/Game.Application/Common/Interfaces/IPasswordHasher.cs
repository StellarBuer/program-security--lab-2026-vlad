namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс сервісу безпечного хешування паролів.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

