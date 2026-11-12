using System.Security.Claims;
using Game.Api.DTOs;
using Game.Application.Identity.Commands;
using Game.Application.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

/// <summary>
/// Контролер автентифікації та керування обліковими записами користувачів.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Реєстрація нового користувача (за замовчуванням призначається роль Player).
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var command = new RegisterUserAC(request.Email, request.Password);
        var userDto = await _authService.RegisterAsync(command, ct);

        return Ok(new
        {
            Message = "Користувач успішно зареєстрований і збережений у базі даних!",
            User = userDto
        });
    }

    /// <summary>
    /// Автентифікація користувача з видачею підписаного JWT-токена.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginUserAC(request.Email, request.Password);
        var authResult = await _authService.LoginAsync(command, ct);

        return Ok(new
        {
            Message = "Автентифікація успішна!",
            Token = authResult.Token,
            User = authResult.User
        });
    }

    /// <summary>
    /// Отримання профілю поточного автентифікованого користувача (перевірка OBAC).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserProfile(CancellationToken ct)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var profile = await _authService.GetProfileAsync(userId, ct);
        return Ok(profile);
    }
}

