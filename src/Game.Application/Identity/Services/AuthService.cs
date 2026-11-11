using Game.Application.Common.Interfaces;
using Game.Application.Identity.Commands;
using Game.Application.Identity.DTOs;
using Game.Domain.Entities;
using Game.Domain.Exceptions;

namespace Game.Application.Identity.Services;

/// <summary>
/// Реалізація сценаріїв автентифікації та розмежування доступу (RBAC + OBAC).
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task<UserResponseDto> RegisterAsync(RegisterUserAC command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            throw new DomainValidationException("Email не може бути порожнім.");

        if (string.IsNullOrWhiteSpace(command.Password))
            throw new DomainValidationException("Пароль не може бути порожнім.");

        if (command.Password.Length < 6)
            throw new WeakPasswordException("довжина пароля має складати не менше 6 символів.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null)
            throw new UserAlreadyExistsException(normalizedEmail);

        var passwordHash = _passwordHasher.HashPassword(command.Password);
        // За замовчуванням призначається роль Player (політика безпеки: заборона самопризначення Admin)
        var user = new UserAccount(normalizedEmail, passwordHash, UserRole.Player);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(user.Id, user.Email, user.Role, user.CreatedAt);
    }

    public async Task<AuthResultDto> LoginAsync(LoginUserAC command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email) || string.IsNullOrWhiteSpace(command.Password))
            throw new DomainValidationException("Email та пароль є обов'язковими.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
            throw new InvalidCredentialsException();

        var isPasswordValid = _passwordHasher.VerifyPassword(command.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new InvalidCredentialsException();

        var token = _tokenService.GenerateToken(user);
        var userDto = new UserResponseDto(user.Id, user.Email, user.Role, user.CreatedAt);

        return new AuthResultDto(token, userDto);
    }

    /// <summary>
    /// Отримання профілю користувача із застосуванням моделі OBAC (Ownership-Based Access Control).
    /// Гравець має право переглядати лише свій профіль, окрім випадків запиту адміністратором (RBAC).
    /// </summary>
    public async Task<UserResponseDto> GetProfileAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        // 1. Перевірка OBAC: чи поточний користувач є власником запитуваного профілю
        if (_currentUserService.UserId != targetUserId && !_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenActionException("Ви не маєте доступу до чужого профілю користувача (порушення моделі OBAC).");
        }

        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(targetUserId);

        return new UserResponseDto(user.Id, user.Email, user.Role, user.CreatedAt);
    }

    /// <summary>
    /// Отримання списку всіх користувачів із застосуванням моделі RBAC (Role-Based Access Control).
    /// Доступно лише користувачам із роллю Admin.
    /// </summary>
    public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAdminAsync(CancellationToken cancellationToken = default)
    {
        // Перевірка RBAC: чи володіє користувач адміністративними привілеями
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenActionException("Ця операція доступна лише адміністраторам системи (порушення моделі RBAC).");
        }

        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => new UserResponseDto(u.Id, u.Email, u.Role, u.CreatedAt)).ToList();
    }
}
