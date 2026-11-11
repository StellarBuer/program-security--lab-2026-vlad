namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс доступу до контексту поточного автентифікованого користувача.
/// Дозволяє шару застосунку перевіряти права власності (OBAC) та системні ролі (RBAC) без прив'язки до HttpContext.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

