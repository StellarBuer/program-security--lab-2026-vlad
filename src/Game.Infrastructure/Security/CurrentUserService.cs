using System.Security.Claims;
using Game.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Game.Infrastructure.Security;

/// <summary>
/// Реалізація сервісу поточного автентифікованого користувача.
/// Зчитує клейми з JWT-токена через IHttpContextAccessor.
/// Використовується шаром Application для RBAC/OBAC перевірок.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string? Email =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;

    /// <inheritdoc />
    /// <remarks>
    /// Повертає рядкове значення ролі з JWT-клейму (напр. "Player" або "Admin").
    /// </remarks>
    public string? Role =>
        _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role)?.Value;

    /// <inheritdoc />
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User.IsInRole(role) == true;
}
