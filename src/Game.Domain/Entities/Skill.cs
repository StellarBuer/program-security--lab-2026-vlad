using Game.Domain.Common;

namespace Game.Domain.Entities;

public enum SkillType
{
    Normal,
    Enhanced,
    Unique
}

public class Skill : ABaseEntity
{
    public string Name { get; private set; }
    public SkillType Type { get; private set; }
    public int PowerMultiplier { get; private set; }

    public Skill(string name, SkillType type, int powerMultiplier)
    {
        Name = name;
        Type = type;
        PowerMultiplier = powerMultiplier;
    }
}