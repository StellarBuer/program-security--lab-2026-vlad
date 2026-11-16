namespace Game.Domain.Exceptions;

/// <summary>
/// Виняток, що сигналізує про порушення ліцензійної угоди:
/// недійсний підпис, модифікований файл ліцензії або використання забороненої функції.
/// </summary>
public sealed class LicenseViolationException : AGameEngineException
{
    public LicenseViolationException(string message)
        : base("CORE", "CORE_LICENSE_VIOLATION", message) { }
}
