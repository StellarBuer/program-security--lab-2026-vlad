using Game.Domain.Entities;

namespace Game.Application.Common.Interfaces;

/// <summary>
/// Інтерфейс репозиторію облікових записів (порт інфраструктури).
/// </summary>
public interface IUserRepository
{
    Task<UserAccount?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(UserAccount user, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

