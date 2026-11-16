using System.Security.Cryptography;
using System.Text;
using Game.Application.Common.Interfaces;

namespace Game.Infrastructure.Security;

/// <summary>
/// Реалізація механізмів контролю цілісності (SHA-256) та підтвердження автентичності (HMAC-SHA256).
/// Усі порівняння криптографічних підписів виконуються в константному часі (CryptographicOperations.FixedTimeEquals)
/// для унеможливлення атак за часом виконання (Timing Attacks).
/// </summary>
public class IntegrityService : IIntegrityService
{
    /// <inheritdoc />
    public string ComputeSha256(string content)
    {
        if (content is null)
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <inheritdoc />
    public bool VerifySha256(string content, string expectedHash)
    {
        if (content is null || string.IsNullOrWhiteSpace(expectedHash))
            return false;

        var computed = ComputeSha256(content);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(expectedHash.Trim().ToLowerInvariant())
        );
    }

    /// <inheritdoc />
    public string ComputeHmacSha256(string payload, string key)
    {
        if (payload is null || key is null)
            return string.Empty;

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        var hmac = HMACSHA256.HashData(keyBytes, payloadBytes);
        return Convert.ToHexString(hmac).ToLowerInvariant();
    }

    /// <inheritdoc />
    public bool VerifyHmacSha256(string payload, string key, string expectedHmac)
    {
        if (payload is null || key is null || string.IsNullOrWhiteSpace(expectedHmac))
            return false;

        var computed = ComputeHmacSha256(payload, key);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(expectedHmac.Trim().ToLowerInvariant())
        );
    }
}
