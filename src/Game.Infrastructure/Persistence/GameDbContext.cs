using Microsoft.EntityFrameworkCore;
using Game.Domain.Entities; // підтягуємо доменні сутності

namespace Game.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Налаштування таблиці користувачів через Fluent API
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique(); // унікальна пошта
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });
    }
}