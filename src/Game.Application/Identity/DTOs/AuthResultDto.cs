namespace Game.Application.Identity.DTOs;

/// <summary>
/// Результат успішної автентифікації/реєстрації з JWT-токеном та даними користувача.
/// </summary>
public record AuthResultDto(string Token, UserResponseDto User);

