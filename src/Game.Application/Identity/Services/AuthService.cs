using Game.Application.Common.Interfaces;
using Game.Application.Identity.Commands;
using Game.Application.Identity.DTOs;
using Game.Domain.Entities;
using Game.Domain.Exceptions;

namespace Game.Application.Identity.Services;

/// <summary>
/// Реалізація сценаріїв автентифікації.
/// Керує валідацією, доменними інваріантами, хешуванням, перевіркою паролів та видачею токенів.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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
        var user = new UserAccount(normalizedEmail, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserResponseDto(user.Id, user.Email, user.CreatedAt);
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
        var userDto = new UserResponseDto(user.Id, user.Email, user.CreatedAt);

        return new AuthResultDto(token, userDto);
    }

    public async Task<UserResponseDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(userId);

        return new UserResponseDto(user.Id, user.Email, user.CreatedAt);
    }
}
