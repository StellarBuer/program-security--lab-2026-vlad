using System.Security.Cryptography;
using System.Text;
using Game.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Game.Infrastructure.Security;

/// <summary>
/// Реалізація симетричного автентифікованого шифрування на базі AES-256-GCM (AEAD).
/// Захищає чутливі дані (Data at Rest) від витоку та модифікації (STRIDE: Information Disclosure & Tampering).
/// Формат шифротексту: enc:v1:{Base64([12 байтів Nonce] + [16 байтів Tag] + [N байтів Ciphertext])}.
/// </summary>
public class AesGcmSymmetricEncryptionService : ISymmetricEncryptionService
{
    private const string PayloadPrefix = "enc:v1:";
    private const int NonceSizeInBytes = 12; // 96 бітів (рекомендований розмір для GCM)
    private const int TagSizeInBytes = 16;   // 128 бітів

    private readonly byte[] _key;

    public AesGcmSymmetricEncryptionService(IConfiguration configuration)
    {
        var base64Key = configuration["Security:Encryption:Key"];

        if (!string.IsNullOrWhiteSpace(base64Key))
        {
            _key = Convert.FromBase64String(base64Key);
            if (_key.Length != 32)
            {
                throw new ArgumentException("Ключ шифрування AES-256 повинен мати довжину рівно 32 байти (256 біт).");
            }
        }
        else
        {
            // Fallback детермінований ключ для локальної розробки, якщо не вказано в appsettings
            _key = SHA256.HashData(Encoding.UTF8.GetBytes("TacticsGameEngineDefaultDevelopmentMasterKey2026!"));
        }
    }

    /// <summary>
    /// Конструктор для прямого передавання ключа (наприклад, для тестування).
    /// </summary>
    public AesGcmSymmetricEncryptionService(byte[] key)
    {
        if (key == null || key.Length != 32)
            throw new ArgumentException("Ключ шифрування AES-256 повинен мати довжину рівно 32 байти (256 біт).", nameof(key));

        _key = key.ToArray();
    }

    /// <inheritdoc />
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = new byte[NonceSizeInBytes];
        var tag = new byte[TagSizeInBytes];
        var cipherBytes = new byte[plainBytes.Length];

        // Криптографічно стійка генерація випадкового Nonce (унікальний для кожного шифрування)
        RandomNumberGenerator.Fill(nonce);

        using (var aesGcm = new AesGcm(_key, TagSizeInBytes))
        {
            aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);
        }

        // Об'єднуємо: [Nonce (12) | Tag (16) | Ciphertext (N)]
        var payload = new byte[NonceSizeInBytes + TagSizeInBytes + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSizeInBytes);
        Buffer.BlockCopy(tag, 0, payload, NonceSizeInBytes, TagSizeInBytes);
        Buffer.BlockCopy(cipherBytes, 0, payload, NonceSizeInBytes + TagSizeInBytes, cipherBytes.Length);

        return $"{PayloadPrefix}{Convert.ToBase64String(payload)}";
    }

    /// <inheritdoc />
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        // Зворотна сумісність: якщо рядок не має префіксу шифротексту, це відкриті дані (наприклад, legacy-записи в БД)
        if (!cipherText.StartsWith(PayloadPrefix, StringComparison.Ordinal))
            return cipherText;

        var base64Part = cipherText.Substring(PayloadPrefix.Length);
        byte[] payload;

        try
        {
            payload = Convert.FromBase64String(base64Part);
        }
        catch (FormatException)
        {
            // Якщо формат не валідний Base64, повертаємо як є
            return cipherText;
        }

        var minPayloadLength = NonceSizeInBytes + TagSizeInBytes;
        if (payload.Length < minPayloadLength)
        {
            throw new CryptographicException("Невалідний розмір зашифрованого повідомлення: пошкоджені або неповні дані.");
        }

        var nonce = new byte[NonceSizeInBytes];
        var tag = new byte[TagSizeInBytes];
        var cipherBytesLength = payload.Length - minPayloadLength;
        var cipherBytes = new byte[cipherBytesLength];
        var plainBytes = new byte[cipherBytesLength];

        Buffer.BlockCopy(payload, 0, nonce, 0, NonceSizeInBytes);
        Buffer.BlockCopy(payload, NonceSizeInBytes, tag, 0, TagSizeInBytes);
        Buffer.BlockCopy(payload, minPayloadLength, cipherBytes, 0, cipherBytesLength);

        try
        {
            using var aesGcm = new AesGcm(_key, TagSizeInBytes);
            aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Помилка цілісності: автентифікаційний тег не збігається. Дані було модифіковано або використано невірний ключ.", ex);
        }

        return Encoding.UTF8.GetString(plainBytes);
    }
}

