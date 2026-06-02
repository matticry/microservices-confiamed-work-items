using ItemsTrabajo.Application.DTOs.WorkItem;
using MediatR;

namespace ItemsTrabajo.Application.Commands.CreateWorkItem;

public record CreateWorkItemCommand(CreateWorkItemDto WorkItem) : IRequest<WorkItemDto>;
