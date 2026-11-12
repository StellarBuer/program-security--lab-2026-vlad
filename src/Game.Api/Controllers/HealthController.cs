using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

/// <summary>
/// Ендпоінт діагностики стану працездатності сервера (Health Check).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Check()
    {
        return Ok(new
        {
            Status = "Online",
            Message = "Tactics Game Server is running!"
        });
    }
}

