using Game.Application.Identity.Commands;
using Game.Application.Identity.DTOs;

namespace Game.Application.Identity.Services;

/// <summary>
/// Інтерфейс сервісу автентифікації та роботи з обліковими записами.
/// </summary>
public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(RegisterUserAC command, CancellationToken cancellationToken = default);
    Task<AuthResultDto> LoginAsync(LoginUserAC command, CancellationToken cancellationToken = default);
    Task<UserResponseDto> GetProfileAsync(Guid targetUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserResponseDto>> GetAllUsersAdminAsync(CancellationToken cancellationToken = default);
}
