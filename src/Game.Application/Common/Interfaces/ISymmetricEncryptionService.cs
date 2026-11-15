namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс сервісу симетричного криптографічного захисту (AEAD: AES-256-GCM).
/// Забезпечує конфіденційність та контроль цілісності даних у стані спокою (Data at Rest).
/// </summary>
public interface ISymmetricEncryptionService
{
    /// <summary>
    /// Шифрує відкритий текст за алгоритмом AES-256-GCM з генерацією унікального Nonce.
    /// </summary>
    /// <param name="plainText">Відкритий текст для шифрування.</param>
    /// <returns>Серіалізований шифротекст із версією ключа, Nonce та Auth Tag.</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// Розшифровує шифротекст за алгоритмом AES-256-GCM та верифікує автентифікаційний тег.
    /// </summary>
    /// <param name="cipherText">Зашифрований рядок.</param>
    /// <returns>Розшифрований вихідний текст.</returns>
    string Decrypt(string cipherText);
}

