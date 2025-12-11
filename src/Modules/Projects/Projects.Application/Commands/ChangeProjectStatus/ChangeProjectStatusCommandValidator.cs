using FluentValidation;

namespace Projects.Application.Commands.ChangeProjectStatus;

/// <summary>
/// Валидатор команды изменения статуса проекта.
/// </summary>
internal sealed class ChangeProjectStatusCommandValidator : AbstractValidator<ChangeProjectStatusCommand>
{
    public ChangeProjectStatusCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid project status.");
    }
}

