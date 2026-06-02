using ItemsTrabajo.Application.Interfaces;
using ItemsTrabajo.Domain.Entities;
using ItemsTrabajo.Domain.Enums;
using MediatR;

namespace ItemsTrabajo.Application.Commands.AssignWorkItem;

public class AssignWorkItemCommandHandler : IRequestHandler<AssignWorkItemCommand, AssignWorkItemResult>
{
    private readonly IWorkItemRepository _workItemRepository;
    private readonly IUserWorkRepository _userWorkRepository;

    public AssignWorkItemCommandHandler(
        IWorkItemRepository workItemRepository,
        IUserWorkRepository userWorkRepository)
    {
        _workItemRepository = workItemRepository;
        _userWorkRepository = userWorkRepository;
    }

    public async Task<AssignWorkItemResult> Handle(AssignWorkItemCommand request, CancellationToken cancellationToken)
    {
        var workItem = await _workItemRepository.GetNextPendingAsync(cancellationToken)
            ?? throw new InvalidOperationException("No pending work items available for assignment.");

        var today = DateTime.UtcNow.Date;
        var isUrgent = workItem.ExpirationDate.HasValue &&
                       (workItem.ExpirationDate.Value.Date - today).TotalDays < 3;

        var users = await _userWorkRepository.GetAllActiveUsersAsync(cancellationToken);

        var candidateUsers = isUrgent
            ? users
            : await FilterNonSaturatedUsersAsync(users, cancellationToken);

        var selectedUser = await SelectUserWithLeastPendingAsync(candidateUsers, cancellationToken)
            ?? throw new InvalidOperationException("No available user found for assignment.");

        var pendingItems = await _userWorkRepository.GetPendingByUserAsync(selectedUser.IdUs, cancellationToken);

        var userWork = new UserWork
        {
            UserId = selectedUser.IdUs,
            ItemId = workItem.IdWi,
            Status = ((int)UserWorkStatus.Pending).ToString(),
            AssignmentDate = DateTime.UtcNow,
            OrderPriority = pendingItems.Count + 1
        };

        await _userWorkRepository.AddAsync(userWork, cancellationToken);

        workItem.StatusWi = ((int)WorkItemStatus.Assigned).ToString();
        await _workItemRepository.UpdateAsync(workItem, cancellationToken);

        await ReorderUserPendingItemsAsync(selectedUser.IdUs, cancellationToken);

        return new AssignWorkItemResult(
            userWork.IdUW,
            selectedUser.UsernameUs!,
            workItem.IdWi,
            workItem.CodeWi!,
            userWork.OrderPriority!.Value);
    }

    private async Task<User?> SelectUserWithLeastPendingAsync(List<User> users, CancellationToken cancellationToken)
    {
        User? selected = null;
        int minPending = int.MaxValue;

        foreach (var user in users)
        {
            var pending = await _userWorkRepository.GetPendingByUserAsync(user.IdUs, cancellationToken);
            if (pending.Count < minPending)
            {
                minPending = pending.Count;
                selected = user;
            }
        }

        return selected;
    }

    private async Task<List<User>> FilterNonSaturatedUsersAsync(List<User> users, CancellationToken cancellationToken)
    {
        var eligible = new List<User>();

        foreach (var user in users)
        {
            var pending = await _userWorkRepository.GetPendingByUserAsync(user.IdUs, cancellationToken);
            var highRelevanceCount = pending.Count(uw => uw.Item?.Relevance == "H");

            if (highRelevanceCount <= 3)
                eligible.Add(user);
        }

        return eligible;
    }

    private async Task ReorderUserPendingItemsAsync(int userId, CancellationToken cancellationToken)
    {
        var pending = await _userWorkRepository.GetPendingByUserAsync(userId, cancellationToken);

        var ordered = pending
            .OrderByDescending(uw => uw.Item?.Relevance == "H")
            .ThenBy(uw => uw.Item?.ExpirationDate)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
            ordered[i].OrderPriority = i + 1;

        await _userWorkRepository.UpdateRangeAsync(ordered, cancellationToken);
    }
}