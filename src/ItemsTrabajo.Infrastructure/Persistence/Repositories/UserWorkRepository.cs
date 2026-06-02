using ItemsTrabajo.Application.Interfaces;
using ItemsTrabajo.Domain.Entities;
using ItemsTrabajo.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ItemsTrabajo.Infrastructure.Persistence.Repositories;

public class UserWorkRepository : IUserWorkRepository
{
    private readonly ApplicationDbContext _context;

    public UserWorkRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserWork>> GetPendingByUserAsync(int userId, CancellationToken cancellationToken = default)
        => await _context.UserWorks
            .Include(uw => uw.Item)
            .Where(uw => uw.UserId == userId && uw.Status == "0")
            .ToListAsync(cancellationToken);

    public async Task<List<User>> GetAllActiveUsersAsync(CancellationToken cancellationToken = default)
        => await _context.Users
            .Where(u => u.StatusUs == "A")
            .ToListAsync(cancellationToken);

    public async Task AddAsync(UserWork userWork, CancellationToken cancellationToken = default)
    {
        _context.UserWorks.Add(userWork);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(List<UserWork> userWorks, CancellationToken cancellationToken = default)
    {
        _context.UserWorks.UpdateRange(userWorks);
        await _context.SaveChangesAsync(cancellationToken);
    }
}