namespace Game.Application.Common.Interfaces;

/// <summary>
/// Порт для управління ліцензією програмного продукту.
/// Підтримує модель розповсюдження Freeware з перевіркою цифрового підпису RSA-PSS SHA-256.
/// </summary>
public interface ILicenseService
{
    /// <summary>
    /// Перевіряє цифровий підпис файла ліцензії (RSA-PSS SHA-256).
    /// Повертає true якщо підпис валідний і ліцензія не була модифікована.
    /// </summary>
    bool VerifyLicenseSignature(string licenseJson, byte[] signature);

    /// <summary>
    /// Розбирає та повертає дані ліцензії після успішної верифікації підпису.
    /// Кидає <see cref="Domain.Exceptions.LicenseViolationException"/> якщо підпис невалідний.
    /// </summary>
    LicenseData LoadAndVerifyLicense(string licenseJson, byte[] signature);

    /// <summary>
    /// Перевіряє чи дозволена певна функція у поточній ліцензії.
    /// </summary>
    bool IsFeatureAllowed(LicenseData license, string featureName);
}

/// <summary>
/// Дані ліцензії програмного продукту (модель Freeware).
/// </summary>
public sealed record LicenseData(
    string LicenseId,
    string ProductId,
    string ProductVersion,
    string Owner,
    DateOnly IssuedAt,
    DateOnly? ExpiresAt,
    string Model,
    IReadOnlyList<string> AllowedFeatures,
    bool BindToAccount,
    bool CommercialUse,
    bool Redistribution,
    bool Modification,
    string? Notes
);
