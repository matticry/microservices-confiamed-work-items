using ItemsTrabajo.Domain.Entities;

namespace ItemsTrabajo.Application.Interfaces;

public interface IWorkItemRepository
{
    Task<WorkItem> CreateAsync(WorkItem workItem, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<WorkItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkItem workItem, CancellationToken cancellationToken = default);
    Task<WorkItem?> GetNextPendingAsync(CancellationToken cancellationToken = default);


}