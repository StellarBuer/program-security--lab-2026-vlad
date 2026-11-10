using Game.Application.Common.Interfaces;
using Game.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Infrastructure.Persistence.Repositories;

/// <summary>
/// Репозиторій для роботи з сутністю UserAccount через Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly GameDbContext _dbContext;

    public UserRepository(GameDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserAccounts
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task AddAsync(UserAccount user, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserAccounts.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

