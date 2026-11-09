using Game.Domain.Common;

namespace Game.Domain.Entities;

public class Character : ABaseEntity
{
    public string Name { get; private set; }
    public int Health { get; private set; }
    public int Attack { get; private set; }
    public int Defense { get; private set; }
    public int Speed { get; private set; }

    // Список навичок персонажа (має бути рівно 3: звичайна, посилена, унікальна)
    private readonly List<Skill> _skills = new();
    public IReadOnlyCollection<Skill> Skills => _skills.AsReadOnly();

    public Character(string name, int health, int attack, int defense, int speed, IEnumerable<Skill> skills)
    {
        Name = name;
        Health = health;
        Attack = attack;
        Defense = defense;
        Speed = speed;
        _skills = skills.ToList();
    }

    // Доменний метод для отримання шкоди (інваріант: здоров'я не менше 0)
    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
    }
}