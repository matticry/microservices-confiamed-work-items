using FluentValidation;
using ItemsTrabajo.Application.DTOs.WorkItem;

namespace ItemsTrabajo.Application.Commands.CreateWorkItem;

public class CreateWorkItemValidator : AbstractValidator<CreateWorkItemDto>
{
    public CreateWorkItemValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.Relevance)
            .NotEmpty().WithMessage("Relevance is required.")
            .Must(r => r == "H" || r == "L")
            .WithMessage("Relevance must be 'H' (High) or 'L' (Low).");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future.");
    }
}