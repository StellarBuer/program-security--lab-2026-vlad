namespace Game.Domain.Exceptions;

/// <summary>
/// Виняток валідації доменних інваріантів у SharedKernel/Core.
/// </summary>
public class DomainValidationException : AGameEngineException
{
    public DomainValidationException(string message)
        : base("CORE", "CORE_VALIDATION_ERROR", message)
    {
    }
}

