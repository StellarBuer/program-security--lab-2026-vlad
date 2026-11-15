namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс сервісу обчислення сліпих індексів (Blind Indexing).
/// Забезпечує детермінований псевдонім (HMAC-SHA256) для швидкого точного пошуку
/// та перевірки унікальності зашифрованих полів без розкриття вихідного тексту.
/// </summary>
public interface IBlindIndexService
{
    /// <summary>
    /// Обчислює детермінований HMAC-хеш для вхідного рядка.
    /// </summary>
    /// <param name="input">Вхідний текст (наприклад, нормалізований email).</param>
    /// <returns>Шістнадцятковий хеш для збереження в індексованій колонці.</returns>
    string ComputeIndex(string input);
}

