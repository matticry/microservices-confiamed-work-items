using MediatR;

namespace ItemsTrabajo.Application.Commands.AssignWorkItem;

public record AssignWorkItemCommand() : IRequest<AssignWorkItemResult>;
public record AssignWorkItemResult(int UserWorkId, string AssignedUsername, int WorkItemId, string WorkItemCode, int OrderPriority);

