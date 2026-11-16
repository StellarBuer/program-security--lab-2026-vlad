namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс сервісу контролю цілісності та підтвердження автентичності даних.
/// Забезпечує перевірку контрольних сум (SHA-256) та автентифікацію повідомлень (HMAC-SHA256).
/// </summary>
public interface IIntegrityService
{
    /// <summary>
    /// Обчислює криптографічний хеш SHA-256 для контролю цілісності статичних даних.
    /// </summary>
    string ComputeSha256(string content);

    /// <summary>
    /// Перевіряє цілісність даних шляхом порівняння обчисленого SHA-256 з еталонним значенням.
    /// </summary>
    bool VerifySha256(string content, string expectedHash);

    /// <summary>
    /// Генерує код автентифікації повідомлення (HMAC-SHA256) для динамічних даних/дій.
    /// </summary>
    string ComputeHmacSha256(string payload, string key);

    /// <summary>
    /// Перевіряє автентичність та цілісність payload через захищене від Timing Attacks порівняння HMAC.
    /// </summary>
    bool VerifyHmacSha256(string payload, string key, string expectedHmac);
}
