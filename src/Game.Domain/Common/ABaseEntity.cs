
namespace Game.Domain.Common;

public abstract class ABaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
