using AutoMapper;
using ItemsTrabajo.Application.DTOs.WorkItem;
using ItemsTrabajo.Application.Interfaces;
using ItemsTrabajo.Domain.Entities;
using ItemsTrabajo.Domain.Enums;
using MediatR;

namespace ItemsTrabajo.Application.Commands.CreateWorkItem;

public class CreateWorkItemHandler : IRequestHandler<CreateWorkItemCommand, WorkItemDto>
{
    private readonly IWorkItemRepository _repository;
    private readonly IMapper _mapper;

    public CreateWorkItemHandler(IWorkItemRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<WorkItemDto> Handle(CreateWorkItemCommand request, CancellationToken cancellationToken)
    {
        // Validar que el código no exista
        var exists = await _repository.ExistsByCodeAsync(request.WorkItem.Code, cancellationToken);
        if (exists)
            throw new InvalidOperationException($"A work item with code '{request.WorkItem.Code}' already exists.");

        var workItem = _mapper.Map<WorkItem>(request.WorkItem);

        // Valores por defecto al crear
        workItem.StatusWi = ((int)WorkItemStatus.Pending).ToString();
        workItem.CreatedAt = DateTime.UtcNow;

        var created = await _repository.CreateAsync(workItem, cancellationToken);

        return _mapper.Map<WorkItemDto>(created);
    }
}