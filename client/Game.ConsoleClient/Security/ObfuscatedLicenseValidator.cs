using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Game.ConsoleClient.Security;

/// <summary>
/// Валідатор ліцензії клієнта із застосуванням методів протидії реверсивній інженерії:
/// - Control Flow Flattening (сплющення графу викликів у скінченний автомат)
/// - Непрозорі предикати (Opaque Predicates)
/// - Сміттєвий код та нелінійні переходи станів
/// - Приховування структури логіки перевірки
/// </summary>
public static class ObfuscatedLicenseValidator
{
    // Непрозорий предикат: математично (n^2 + n) завжди парне для будь-якого n
    private static bool OpaqueAlwaysTrue(int n) => ((n * n + n) & 1) == 0;

    // Публічний ключ розробника (RSA-3072) для верифікації
    private const string PubKeyPem = """
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

    /// <summary>
    /// Валідує ліцензію через обфускований автомат станів.
    /// </summary>
    public static bool ValidateClientLicense(string licenseJson, byte[] signature)
    {
        var state = 0x1A;
        var accumulator = 0x42;
        var isSignatureValid = false;
        var areRulesValid = false;

        while (state != 0xFF)
        {
            switch (state)
            {
                case 0x1A: // Початковий стан: ініціалізація та непрозорий перехід
                    accumulator ^= (state * 3);
                    state = OpaqueAlwaysTrue(accumulator) ? 0x2B : 0x99;
                    break;

                case 0x2B: // Перевірка цифрового підпису RSA-PSS
                    try
                    {
                        using var rsa = RSA.Create();
                        rsa.ImportFromPem(PubKeyPem);
                        var data = Encoding.UTF8.GetBytes(licenseJson);
                        isSignatureValid = rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
                    }
                    catch
                    {
                        isSignatureValid = false;
                    }
                    state = 0x3C;
                    break;

                case 0x3C: // Сміттєвий розрахунок (Dead Code) для ускладнення CFG
                    accumulator = (accumulator << 2) ^ 0x55AA;
                    state = 0x4D;
                    break;

                case 0x4D: // Розбір параметрів ліцензії JSON
                    if (isSignatureValid)
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(licenseJson);
                            var root = doc.RootElement;
                            var model = root.GetProperty("model").GetString();
                            var features = root.GetProperty("allowedFeatures")
                                .EnumerateArray()
                                .Select(e => e.GetString())
                                .ToHashSet(StringComparer.OrdinalIgnoreCase);

                            var expectedModel = ObfuscatedStrings.GetModelName();
                            var requiredFeature = ObfuscatedStrings.GetRequiredFeature();

                            areRulesValid = string.Equals(model, expectedModel, StringComparison.Ordinal)
                                            && features.Contains(requiredFeature);
                        }
                        catch
                        {
                            areRulesValid = false;
                        }
                    }
                    state = 0x5E;
                    break;

                case 0x5E: // Фіналізація перевірки
                    state = 0xFF; // Вихід
                    break;

                case 0x99: // Фальшива гілка (ніколи не досягається завдяки непрозорому предикату)
                    isSignatureValid = true;
                    areRulesValid = true;
                    state = 0xFF;
                    break;

                default:
                    state = 0xFF;
                    break;
            }
        }

        return isSignatureValid && areRulesValid;
    }
}
