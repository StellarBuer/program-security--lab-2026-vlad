using Game.Application.Common.Interfaces;
using Game.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторій для роботи з сутністю UserAccount через Entity Framework Core
/// з підтримкою сліпого індексування для зашифрованих полів.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly GameDbContext _dbContext;
    private readonly IBlindIndexService _blindIndexService;

    public UserRepository(GameDbContext dbContext, IBlindIndexService blindIndexService)
    {
        _dbContext = dbContext;
        _blindIndexService = blindIndexService;
    }

    /// <summary>
    /// Отримує користувача за email шляхом обчислення його сліпого індексу (Blind Index).
    /// Забезпечує пошук за індексом O(1) без розкриття зашифрованих даних у SQL-запиті.
    /// </summary>
    public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var blindIndex = _blindIndexService.ComputeIndex(normalized);

        // Пошук за Blind Index (із підтримкою legacy відкритих email для зворотної сумісності)
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.EmailBlindIndex == blindIndex || u.Email == normalized, cancellationToken);
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<UserAccount>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAccounts.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        // Якщо Blind Index не встановлено, генеруємо його перед додаванням до контексту
        if (string.IsNullOrEmpty(user.EmailBlindIndex))
        {
            var blindIndex = _blindIndexService.ComputeIndex(user.Email);
            user.SetEmailBlindIndex(blindIndex);
        }

        await _dbContext.UserAccounts.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
