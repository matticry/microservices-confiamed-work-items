
using Microsoft.EntityFrameworkCore;
using ItemsTrabajo.Application.Interfaces;
using ItemsTrabajo.Domain.Entities;
using ItemsTrabajo.Domain.Enums;
using ItemsTrabajo.Infrastructure.Context;

namespace ItemsTrabajo.Infrastructure.Persistence.Repositories;

public class WorkItemRepository : IWorkItemRepository
{
    private readonly ApplicationDbContext _context;

    public WorkItemRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<WorkItem?> GetNextPendingAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.WorkItems
            .Where(wi => wi.StatusWi == ((int)WorkItemStatus.Pending).ToString())
            .OrderByDescending(wi => wi.ExpirationDate.HasValue && 
                                     EF.Functions.DateDiffDay(today, wi.ExpirationDate.Value.Date) < 3)
            .ThenByDescending(wi => wi.Relevance == "H")
            .ThenBy(wi => wi.ExpirationDate)
            .FirstOrDefaultAsync(cancellationToken);
    }


    public async Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.WorkItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdWi == id, cancellationToken);

    public async Task<WorkItem?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _context.WorkItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CodeWi == code, cancellationToken);

    public async Task<IEnumerable<WorkItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.WorkItems
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<WorkItem>> GetPendingItemsAsync(CancellationToken cancellationToken = default)
        => await _context.WorkItems
            .AsNoTracking()
            .Where(x => x.StatusWi == ((int)WorkItemStatus.Pending).ToString())
            .ToListAsync(cancellationToken);

    public async Task<WorkItem> CreateAsync(WorkItem workItem, CancellationToken cancellationToken = default)
    {
        _context.WorkItems.Add(workItem);
        await _context.SaveChangesAsync(cancellationToken);
        return workItem;
    }

    public async Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default)
    {
        _context.WorkItems.Update(workItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var workItem = await _context.WorkItems.FindAsync([id], cancellationToken);
        if (workItem is null) return;

        _context.WorkItems.Remove(workItem);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
        => await _context.WorkItems
            .AnyAsync(x => x.CodeWi == code, cancellationToken);
}