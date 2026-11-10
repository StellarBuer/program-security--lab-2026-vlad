namespace Game.Application.Identity.Commands;

/// <summary>
/// Команда реєстрації нового користувача (Application Command).
/// </summary>
public record RegisterUserAC(string Email, string Password);

