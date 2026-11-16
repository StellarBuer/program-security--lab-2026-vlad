using Game.Application.Common.Interfaces;
using Game.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

public record VerifyConfigDto(string Content, string ExpectedHash);
public record SignActionDto(string ActionPayload, string SessionKey);
public record VerifyActionDto(string ActionPayload, string SessionKey, string Signature);

/// <summary>
/// Контролер перевірки цілісності (SHA-256) та автентичності (HMAC-SHA256) даних ігрового рушія.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IntegrityController : ControllerBase
{
    private readonly IIntegrityService _integrityService;

    public IntegrityController(IIntegrityService integrityService)
    {
        _integrityService = integrityService;
    }

    /// <summary>
    /// Отримує еталонну конфігурацію ігрового поля 5x5 та її еталонний SHA-256 хеш цілісності.
    /// </summary>
    [HttpGet("sample-config")]
    public IActionResult GetSampleConfig()
    {
        var canonicalConfig = "GridSize:5x5;MaxCharacters:4;PlacementZone:3x3;AllowedRoles:Player,Admin";
        var hash = _integrityService.ComputeSha256(canonicalConfig);

        return Ok(new
        {
            Configuration = canonicalConfig,
            Sha256Hash = hash
        });
    }

    /// <summary>
    /// Перевіряє цілісність конфігурації. У разі розбіжності відхиляє запит.
    /// </summary>
    [HttpPost("verify-config")]
    public IActionResult VerifyConfig([FromBody] VerifyConfigDto request)
    {
        var isValid = _integrityService.VerifySha256(request.Content, request.ExpectedHash);

        if (!isValid)
        {
            throw new DataIntegrityViolationException("Невідповідність контрольної суми: конфігурація була модифікована або пошкоджена.");
        }

        return Ok(new { Status = "Valid", Message = "Цілісність конфігурації успішно підтверджено." });
    }

    /// <summary>
    /// Генерує HMAC-SHA256 підпис для дії гравця з використанням сесійного ключа.
    /// </summary>
    [HttpPost("sign-action")]
    public IActionResult SignAction([FromBody] SignActionDto request)
    {
        var signature = _integrityService.ComputeHmacSha256(request.ActionPayload, request.SessionKey);

        return Ok(new
        {
            Payload = request.ActionPayload,
            Signature = signature
        });
    }

    /// <summary>
    /// Перевіряє автентичність та незмінність дії гравця через HMAC-SHA256 (Constant-Time).
    /// </summary>
    [HttpPost("verify-action")]
    public IActionResult VerifyAction([FromBody] VerifyActionDto request)
    {
        var isValid = _integrityService.VerifyHmacSha256(request.ActionPayload, request.SessionKey, request.Signature);

        if (!isValid)
        {
            throw new DataIntegrityViolationException("Помилка автентичності: підпис дії не збігається. Можливо спроба підробки або атака повторного відтворення.");
        }

        return Ok(new { Status = "Valid", Message = "Автентичність та цілісність дії гравця підтверджено." });
    }
}
