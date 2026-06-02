using ItemsTrabajo.Domain.Entities;

namespace ItemsTrabajo.Application.Interfaces;

public interface IUserWorkRepository
{
    Task<List<UserWork>> GetPendingByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllActiveUsersAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserWork userWork, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(List<UserWork> userWorks, CancellationToken cancellationToken = default);
}