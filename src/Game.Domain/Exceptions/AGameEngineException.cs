namespace Game.Domain.Exceptions;

/// <summary>
/// Головний абстрактний виняток ігрового рушія.
/// Усі доменні, бізнес- та аплікаційні помилки повинні успадковуватися від нього.
/// </summary>
public abstract class AGameEngineException : Exception
{
    /// <summary>
    /// Код контексту або модуля (наприклад: CORE, IDNT, BTTL, PROG).
    /// </summary>
    public string ContextCode { get; }

    /// <summary>
    /// Унікальний ідентифікатор помилки для клієнта (наприклад: IDNT_USER_ALREADY_EXISTS).
    /// </summary>
    public string ErrorCode { get; }

    protected AGameEngineException(string contextCode, string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ContextCode = contextCode;
        ErrorCode = errorCode;
    }
}

