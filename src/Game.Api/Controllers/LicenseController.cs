using Game.Application.Common.Interfaces;
using Game.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

public record VerifyLicenseRequest(string LicenseJson, string SignatureBase64);

/// <summary>
/// Контролер перевірки ліцензії програмного продукту.
/// Модель розповсюдження: Freeware (RSA-PSS SHA-256 цифровий підпис).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly ILicenseService _licenseService;
    private readonly ILogger<LicenseController> _logger;

    public LicenseController(ILicenseService licenseService, ILogger<LicenseController> logger)
    {
        _licenseService = licenseService;
        _logger = logger;
    }

    /// <summary>
    /// Повертає поточну ліцензію у вигляді JSON та її base64-підпис (для демонстрації).
    /// У реальному продукті підпис постачається разом із дистрибутивом.
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetLicenseInfo()
    {
        var licenseJson = GetEmbeddedLicenseJson();
        var signatureBase64 = GetEmbeddedSignatureBase64();
        var signature = Convert.FromBase64String(signatureBase64);

        var licenseData = _licenseService.LoadAndVerifyLicense(licenseJson, signature);

        return Ok(new
        {
            Status = "Valid",
            License = new
            {
                licenseData.LicenseId,
                licenseData.ProductId,
                licenseData.ProductVersion,
                licenseData.Owner,
                licenseData.Model,
                licenseData.IssuedAt,
                licenseData.ExpiresAt,
                licenseData.AllowedFeatures,
                licenseData.CommercialUse,
                licenseData.Redistribution,
                licenseData.Modification,
                licenseData.Notes
            },
            SignatureAlgorithm = "RSA-PSS SHA-256 (3072-bit key)",
            Message = "Ліцензія дійсна. Цифровий підпис підтверджено."
        });
    }

    /// <summary>
    /// Перевіряє підпис довільного license.json (POST).
    /// Дозволяє перевірити як валідний, так і модифікований файл ліцензії.
    /// </summary>
    [HttpPost("verify")]
    public IActionResult VerifyLicense([FromBody] VerifyLicenseRequest request)
    {
        byte[] signature;
        try
        {
            signature = Convert.FromBase64String(request.SignatureBase64);
        }
        catch
        {
            return BadRequest(new { Error = "Невалідний формат підпису (очікується Base64)." });
        }

        var isValid = _licenseService.VerifyLicenseSignature(request.LicenseJson, signature);

        if (!isValid)
        {
            _logger.LogWarning("License signature verification FAILED. Possible tampering detected.");
            throw new LicenseViolationException(
                "Недійсний цифровий підпис ліцензії. Файл ліцензії був модифікований або підмінений.");
        }

        var licenseData = _licenseService.LoadAndVerifyLicense(request.LicenseJson, signature);
        return Ok(new
        {
            Status = "Valid",
            LicenseId = licenseData.LicenseId,
            Owner = licenseData.Owner,
            Model = licenseData.Model,
            AllowedFeatures = licenseData.AllowedFeatures,
            Message = "Цифровий підпис RSA-PSS SHA-256 успішно верифіковано."
        });
    }

    /// <summary>
    /// Перевіряє чи дозволена певна функція за поточною ліцензією.
    /// </summary>
    [HttpGet("feature/{featureName}")]
    public IActionResult CheckFeature(string featureName)
    {
        var licenseJson = GetEmbeddedLicenseJson();
        var signature = Convert.FromBase64String(GetEmbeddedSignatureBase64());
        var licenseData = _licenseService.LoadAndVerifyLicense(licenseJson, signature);

        var allowed = _licenseService.IsFeatureAllowed(licenseData, featureName);
        return Ok(new
        {
            Feature = featureName,
            Allowed = allowed,
            Model = licenseData.Model,
            Message = allowed
                ? $"Функція '{featureName}' дозволена ліцензією {licenseData.Model}."
                : $"Функція '{featureName}' не передбачена поточною ліцензією."
        });
    }

    // ---------------------------------------------------------------------------
    // Вбудовані ліцензійні дані (Base64 гарантує точність байтів незалежно від CRLF/LF)
    // ---------------------------------------------------------------------------
    private static readonly string EmbeddedLicenseJsonBase64 =
        "ewogICJsaWNlbnNlSWQiOiAiTElDLTIwMjYtVEdFTkdJTkUtMDAwMDEiLAogICJwcm9kdWN0SWQiOiAidGFjdGljcy1nYW1lLWVuZ2luZSIsCiAgInByb2R1Y3RWZXJzaW9uIjogIjEuMC4wIiwKICAib3duZXIiOiAiVmxhZHlzbGF2IiwKICAiaXNzdWVkQXQiOiAiMjAyNi0xMS0xNiIsCiAgImV4cGlyZXNBdCI6IG51bGwsCiAgIm1vZGVsIjogIkZyZWV3YXJlIiwKICAiYWxsb3dlZEZlYXR1cmVzIjogWwogICAgImJhdHRsZS1lbmdpbmUiLAogICAgImNoYXJhY3Rlci1tYW5hZ2VtZW50IiwKICAgICJpZGVudGl0eS1hdXRoIiwKICAgICJwcm9ncmVzcy10cmFja2luZyIKICBdLAogICJtYXhEZXZpY2VzIjogbnVsbCwKICAiYmluZFRvQWNjb3VudCI6IHRydWUsCiAgImNvbW1lcmNpYWxVc2UiOiBmYWxzZSwKICAicmVkaXN0cmlidXRpb24iOiBmYWxzZSwKICAibW9kaWZpY2F0aW9uIjogZmFsc2UsCiAgIm5vdGVzIjogIkZyZWUgZm9yIHBlcnNvbmFsLCBlZHVjYXRpb25hbCBhbmQgbm9uLWNvbW1lcmNpYWwgdXNlIG9ubHkuIE5vIGV4cGlyeS4gQWxsIGZlYXR1cmVzIGVuYWJsZWQuIgp9Cg==";

    private static string GetEmbeddedLicenseJson() =>
        System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(EmbeddedLicenseJsonBase64));

    private static string GetEmbeddedSignatureBase64() =>
        "oByU2eM8D8Q9O0Veo0QImTdECf9c7qBkRomnTuiSn3FO04hgNIvtVwTwfh1Qm4h7huT5lG58WxcTkk+wyvNuSlOjatZrew6BolOoDceIdVlWBdR5Q8TtPepmz/3/" +
        "pbNGJ+IXGcqM2xX4nmgNDpduo5Y63Yk2gTD9EUKD2sRPsKJyjZFp4UVEKR+RrzQG7OFnLgiGkpSZ1+yuMuLJDxdp0G3W97rb+8HL71YowZtBXjPDtyDLPqtcT94ae" +
        "OfsBavo2Aq9iIx2b3kPgA2OtAu9u/78dAO2GrRtbDXUjR5YUPXSB6KtEhovwho5pnSD6mL7A8/7eN/VUhGSQN3SOuEWwVN0V6XhVQijiOQgYbFrY3wK/lgYHe6/Yf" +
        "Iu4MQU8nBLJhD12KwL+6GYVK0anBKxbIjz4cS4IAtfUT8Nqzn83UthyTrZq/f791Zgkn49Eluvd9R0iWJ5Fq2KmU02w1/oK0CYR0UC9m/BweqXibwhKuLWWz67vYj1daYLKVtum83R";
}
