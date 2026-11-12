using Game.Application.Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

/// <summary>
/// Контролер адміністративних дій (захищений системною RBAC політикою AdminOnly).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase
{
    private readonly IAuthService _authService;

    public AdminController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Отримання переліку всіх зареєстрованих користувачів платформи.
    /// Доступно виключно адміністраторам системи.
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var users = await _authService.GetAllUsersAdminAsync(ct);
        return Ok(users);
    }
}

