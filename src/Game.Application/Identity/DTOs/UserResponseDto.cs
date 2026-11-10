namespace Game.Application.Identity.DTOs;

/// <summary>
/// DTO відповіді з публічними даними облікового запису.
/// </summary>
public record UserResponseDto(Guid Id, string Email, DateTime CreatedAt);

