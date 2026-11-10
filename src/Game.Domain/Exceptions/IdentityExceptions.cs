namespace Game.Domain.Exceptions;

/// <summary>
/// Базовий клас для помилок модуля Identity.
/// </summary>
public abstract class IdentityException : AGameEngineException
{
    protected IdentityException(string errorCode, string message, Exception? innerException = null)
        : base("IDNT", errorCode, message, innerException)
    {
    }
}

/// <summary>
/// Виникає при спробі повторної реєстрації з вже існуючим Email.
/// </summary>
public class UserAlreadyExistsException : IdentityException
{
    public UserAlreadyExistsException(string email)
        : base("IDNT_USER_ALREADY_EXISTS", $"Користувач із електронною поштою '{email}' вже зареєстрований.")
    {
    }
}

/// <summary>
/// Виникає при невірних облікових даних.
/// </summary>
public class InvalidCredentialsException : IdentityException
{
    public InvalidCredentialsException()
        : base("IDNT_INVALID_CREDENTIALS", "Невірний email або пароль.")
    {
    }
}

/// <summary>
/// Виникає, якщо користувача не знайдено.
/// </summary>
public class UserNotFoundException : IdentityException
{
    public UserNotFoundException(Guid userId)
        : base("IDNT_USER_NOT_FOUND", $"Користувача з ідентифікатором '{userId}' не знайдено.")
    {
    }
}

/// <summary>
/// Виникає при невідповідності пароля вимогам безпеки.
/// </summary>
public class WeakPasswordException : IdentityException
{
    public WeakPasswordException(string reason)
        : base("IDNT_WEAK_PASSWORD", $"Пароль не відповідає вимогам безпеки: {reason}")
    {
    }
}
