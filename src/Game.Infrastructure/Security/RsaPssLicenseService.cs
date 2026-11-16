using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Game.Application.Common.Interfaces;
using Game.Domain.Exceptions;

namespace Game.Infrastructure.Security;

/// <summary>
/// Реалізація ліцензійного сервісу для моделі Freeware.
/// Верифікація цілісності та автентичності ліцензії виконується через RSA-PSS SHA-256
/// із вбудованим відкритим ключем розробника (3072 біт).
/// Закритий ключ зберігається виключно у захищеному середовищі розробника.
/// </summary>
public sealed class RsaPssLicenseService : ILicenseService
{
    // Відкритий ключ розробника (RSA-3072, SubjectPublicKeyInfo PEM).
    // Вбудований у клієнт — лише для верифікації, не може бути використаний для підпису.
    private const string DeveloperPublicKeyPem = """
        -----BEGIN PUBLIC KEY-----
        MIIBojANBgkqhkiG9w0BAQEFAAOCAY8AMIIBigKCAYEAu9kOL/ZJqauRsxZ9mse5
        r64ON3l+rLangDLeFbq/DJ2oJ+doToaypVC2gCXzTZYwCE54rDpIh1Fc3MD4gxu2
        p60yZS6go6Dwrv84JlG3gi0A6gV7wjoMVaMzuY+L4n4LwQbIj/qt+teF50FiBhjH
        lMmA5dNOdXW+FzN7lFPbTde/Ya0PWpCES7V8CLUNuH8ixphB5KI6Nmcss+3BysEq
        Me9i6n7va1dYkjt5t6cmVMULDF076XLf+FY8Tyu+l/gsd7uE/+yQSUnitB3jzA2s
        bZ7YU1UzumC4ATuOO+IiXeqs3AVItn1G2oeRMNzHujTmNaGzDxTYvXlTR6uaC0F+
        m9F2qddABg/X5mAApQwUG4Z/XRuN9xQ6wSc0sF6lBBirPLxzrjuHzS3gs7KQhe1x
        UXWuWFivEcCel0fKS1PrVIxA75PjoSfcgEMXjGAHlfUlMeJLQvmkBPzjDY1KvobM
        PrWHKPeWxeRB1NZURgRSkabV2mm1EZ4ncUr4isdyfGs1AgMBAAE=
        -----END PUBLIC KEY-----
        """;

    /// <inheritdoc />
    public bool VerifyLicenseSignature(string licenseJson, byte[] signature)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(DeveloperPublicKeyPem);

            var licenseBytes = Encoding.UTF8.GetBytes(licenseJson);

            return rsa.VerifyData(
                licenseBytes,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pss);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public LicenseData LoadAndVerifyLicense(string licenseJson, byte[] signature)
    {
        if (!VerifyLicenseSignature(licenseJson, signature))
        {
            throw new LicenseViolationException(
                "Недійсний цифровий підпис ліцензії. Файл ліцензії був модифікований або підмінений.");
        }

        return ParseLicense(licenseJson);
    }

    /// <inheritdoc />
    public bool IsFeatureAllowed(LicenseData license, string featureName) =>
        license.AllowedFeatures.Contains(featureName, StringComparer.OrdinalIgnoreCase);

    private static LicenseData ParseLicense(string licenseJson)
    {
        using var doc = JsonDocument.Parse(licenseJson);
        var root = doc.RootElement;

        var features = root.GetProperty("allowedFeatures")
            .EnumerateArray()
            .Select(e => e.GetString()!)
            .ToList()
            .AsReadOnly();

        DateOnly? expiresAt = null;
        if (root.TryGetProperty("expiresAt", out var expiresElem)
            && expiresElem.ValueKind == JsonValueKind.String
            && DateOnly.TryParse(expiresElem.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var exp))
        {
            expiresAt = exp;
        }

        return new LicenseData(
            LicenseId: root.GetProperty("licenseId").GetString()!,
            ProductId: root.GetProperty("productId").GetString()!,
            ProductVersion: root.GetProperty("productVersion").GetString()!,
            Owner: root.GetProperty("owner").GetString()!,
            IssuedAt: DateOnly.Parse(root.GetProperty("issuedAt").GetString()!, System.Globalization.CultureInfo.InvariantCulture),
            ExpiresAt: expiresAt,
            Model: root.GetProperty("model").GetString()!,
            AllowedFeatures: features,
            BindToAccount: root.GetProperty("bindToAccount").GetBoolean(),
            CommercialUse: root.GetProperty("commercialUse").GetBoolean(),
            Redistribution: root.GetProperty("redistribution").GetBoolean(),
            Modification: root.GetProperty("modification").GetBoolean(),
            Notes: root.TryGetProperty("notes", out var notesElem)
                ? notesElem.GetString()
                : null
        );
    }
}
