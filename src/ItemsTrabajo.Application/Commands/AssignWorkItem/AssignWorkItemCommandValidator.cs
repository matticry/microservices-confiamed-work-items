using FluentValidation;

namespace ItemsTrabajo.Application.Commands.AssignWorkItem;

public class AssignWorkItemCommandValidator : AbstractValidator<AssignWorkItemCommand>
{
    public AssignWorkItemCommandValidator() { }
}