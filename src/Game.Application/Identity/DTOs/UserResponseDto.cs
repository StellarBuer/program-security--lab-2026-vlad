using Game.Domain.Entities;

namespace Game.Application.Identity.DTOs;

/// <summary>
/// DTO відповіді з публічними даними облікового запису (включаючи роль RBAC).
/// </summary>
public record UserResponseDto(Guid Id, string Email, UserRole Role, DateTime CreatedAt);
