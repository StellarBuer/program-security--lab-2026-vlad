using System.Security.Cryptography;
using System.Text;
using Game.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Game.Infrastructure.Security;

/// <summary>
/// Реалізація сліпого індексування на базі алгоритму HMAC-SHA256.
/// Дозволяє виконувати точний пошук та перевірку унікальності по зашифрованих полях (Email)
/// без їх попереднього дешифрування в оперативну пам'ять (запобігає повному скануванню таблиці).
/// </summary>
public class HmacBlindIndexService : IBlindIndexService
{
    private readonly byte[] _key;

    public HmacBlindIndexService(IConfiguration configuration)
    {
        var base64Key = configuration["Security:BlindIndex:Key"];

        if (!string.IsNullOrWhiteSpace(base64Key))
        {
            _key = Convert.FromBase64String(base64Key);
            if (_key.Length < 16)
            {
                throw new ArgumentException("Ключ для Blind Index повинен мати довжину не менше 16 байтів.");
            }
        }
        else
        {
            // Fallback детермінований ключ для локальної розробки
            _key = SHA256.HashData(Encoding.UTF8.GetBytes("TacticsGameEngineDefaultDevelopmentBlindIndexKey2026!"));
        }
    }

    /// <summary>
    /// Конструктор для прямого передавання ключа (наприклад, для тестів).
    /// </summary>
    public HmacBlindIndexService(byte[] key)
    {
        if (key == null || key.Length < 16)
            throw new ArgumentException("Ключ для Blind Index повинен мати довжину не менше 16 байтів.", nameof(key));

        _key = key.ToArray();
    }

    /// <inheritdoc />
    public string ComputeIndex(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Нормалізація рядка для детермінованого порівняння (case-insensitive)
        var normalized = input.Trim().ToLowerInvariant();
        var inputBytes = Encoding.UTF8.GetBytes(normalized);

        var hash = HMACSHA256.HashData(_key, inputBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

