using Game.Application.Common.Interfaces;

namespace Game.Infrastructure.Security;

/// <summary>
/// Реалізація хешування паролів за допомогою алгоритму BCrypt із солінням.
/// Відповідає вимогам політики безпеки security_policy.md.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 11;

    public string HashPassword(string password)
    {
        // EnhancedHashPassword автоматично генерує криптографічну сіль та застосовує BCrypt
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(password, passwordHash);
    }
}

