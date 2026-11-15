using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Game.Application.Common.Interfaces;
using Game.Domain.Entities;

namespace Game.Infrastructure.Persistence;

public class GameDbContext : DbContext
{
    private readonly ISymmetricEncryptionService? _encryptionService;

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public GameDbContext(
        DbContextOptions<GameDbContext> options,
        ISymmetricEncryptionService? encryptionService = null) : base(options)
    {
        _encryptionService = encryptionService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ValueConverter для прозорого шифрування/дешифрування Email через AES-256-GCM
        var emailEncryptionConverter = new ValueConverter<string, string>(
            plainText => _encryptionService != null ? _encryptionService.Encrypt(plainText) : plainText,
            cipherText => _encryptionService != null ? _encryptionService.Decrypt(cipherText) : cipherText
        );

        // Налаштування таблиці користувачів через Fluent API
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Поле Email шифрується симетричним алгоритмом AES-256-GCM (Data at Rest)
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasConversion(emailEncryptionConverter);

            // Blind Index (HMAC-SHA256) для швидкого пошуку та забезпечення унікальності без дешифрування
            entity.Property(e => e.EmailBlindIndex)
                  .IsRequired()
                  .HasDefaultValue(string.Empty);
            entity.HasIndex(e => e.EmailBlindIndex);

            entity.Property(e => e.PasswordHash).IsRequired();

            // Роль зберігається як ціле число (int) у БД.
            entity.Property(e => e.Role)
                  .IsRequired()
                  .HasConversion<int>();
        });
    }
}
