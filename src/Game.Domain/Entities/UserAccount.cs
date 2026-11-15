using Game.Domain.Common;
using Game.Domain.Exceptions;

namespace Game.Domain.Entities;

public class UserAccount : ABaseEntity
{
    public string Email { get; private set; }

    /// <summary>
    /// Сліпий індекс (Blind Index) на базі HMAC-SHA256 для точного пошуку та контролю унікальності.
    /// Дозволяє виконувати SELECT по базі без розкриття зашифрованого поля Email.
    /// </summary>
    public string EmailBlindIndex { get; private set; }

    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Конструктор для створення нового користувача
    public UserAccount(string email, string passwordHash, UserRole role = UserRole.Player, string? emailBlindIndex = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainValidationException("Email не може бути порожнім.");
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException("Хеш пароля не може бути порожнім.");

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role;
        EmailBlindIndex = emailBlindIndex ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetEmailBlindIndex(string blindIndex)
    {
        if (string.IsNullOrWhiteSpace(blindIndex))
            throw new DomainValidationException("Blind index не може бути порожнім.");

        EmailBlindIndex = blindIndex;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
    }

    // EF Core потребує порожнього конструктора для мапінгу з бази даних
#pragma warning disable CS8618
    protected UserAccount() { }
#pragma warning restore CS8618
}
