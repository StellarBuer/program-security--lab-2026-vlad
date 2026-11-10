namespace Game.Application.Identity.Commands;

/// <summary>
/// Команда авторизації користувача за поштою та паролем.
/// </summary>
public record LoginUserAC(string Email, string Password);

