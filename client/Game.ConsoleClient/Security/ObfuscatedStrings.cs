using System.Text;

namespace Game.ConsoleClient.Security;

/// <summary>
/// Механізм приховування чутливих рядкових констант (String Obfuscation).
/// Запобігає статичному аналізу через утиліти strings, grep або декомпілятори.
/// Рядки збираються в пам'яті динамічно з маскованих байтів лише в момент виклику.
/// </summary>
public static class ObfuscatedStrings
{
    private const byte MaskKey = 0x5A;

    // Маскований рядок: "http://127.0.0.1:5249/api"
    private static readonly byte[] MaskedApiUrl =
    [
        0x32, 0x2E, 0x2E, 0x2A, 0x60, 0x75, 0x75, 0x6B, 0x68, 0x6D,
        0x74, 0x6A, 0x74, 0x6A, 0x74, 0x6B, 0x60, 0x6F, 0x68, 0x6E,
        0x63, 0x75, 0x3B, 0x2A, 0x33
    ];

    // Маскований рядок: "Freeware"
    private static readonly byte[] MaskedModelFreeware =
    [
        0x1C, 0x28, 0x3F, 0x3F, 0x2D, 0x3B, 0x28, 0x3F
    ];

    // Маскований рядок: "battle-engine"
    private static readonly byte[] MaskedFeatureBattle =
    [
        0x38, 0x3B, 0x2E, 0x2E, 0x36, 0x3F, 0x77, 0x3F, 0x34, 0x3D,
        0x33, 0x34, 0x3F
    ];

    public static string GetApiUrl() => Unmask(MaskedApiUrl);
    public static string GetModelName() => Unmask(MaskedModelFreeware);
    public static string GetRequiredFeature() => Unmask(MaskedFeatureBattle);

    private static string Unmask(byte[] masked)
    {
        var buffer = new byte[masked.Length];
        for (var i = 0; i < masked.Length; i++)
        {
            buffer[i] = (byte)(masked[i] ^ MaskKey);
        }
        return Encoding.UTF8.GetString(buffer);
    }
}
