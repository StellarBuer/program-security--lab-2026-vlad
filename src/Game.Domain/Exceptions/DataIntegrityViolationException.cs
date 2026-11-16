namespace Game.Domain.Exceptions;

/// <summary>
/// Доменний виняток порушення цілісності або автентичності даних.
/// Викидається при виявленні невідповідності контрольних сум (SHA-256),
/// підроблених HMAC-підписів або порушенні AEAD тегу автентичності (AES-GCM Auth Tag).
/// </summary>
public class DataIntegrityViolationException : AGameEngineException
{
    public DataIntegrityViolationException(
        string message = "Порушення контролю цілісності: виявлено несанкціоновану модифікацію або підробку даних.",
        Exception? innerException = null)
        : base("CORE", "CORE_INTEGRITY_VIOLATION", message, innerException)
    {
    }
}
