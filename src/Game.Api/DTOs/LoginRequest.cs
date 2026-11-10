namespace Game.Api.DTOs;

/// <summary>
/// Запит на автентифікацію через API.
/// </summary>
public record LoginRequest(string Email, string Password);

