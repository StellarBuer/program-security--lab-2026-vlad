namespace Game.Domain.Entities;

using Game.Domain.Common;

public class UserAccount : ABaseEntity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Конструктор для створення нового користувача
    public UserAccount(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не може бути порожнім.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Хеш пароля не може бути порожнім.", nameof(passwordHash));

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    // EF Core потребує порожнього конструктора для мапінгу з бази даних
#pragma warning disable CS8618
    protected UserAccount() { }
#pragma warning restore CS8618
}