namespace Game.Domain.Exceptions;

/// <summary>
/// Виникає при спробі виконати дію над чужим об'єктом (порушення OBAC) або без достатніх привілеїв (RBAC).
/// Транслюється у статус HTTP 403 Forbidden.
/// </summary>
public class ForbiddenActionException : AGameEngineException
{
    public ForbiddenActionException(string message)
        : base("CORE", "CORE_FORBIDDEN_ACTION", message)
    {
    }
}

