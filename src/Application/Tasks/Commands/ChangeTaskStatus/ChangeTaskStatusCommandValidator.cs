using FluentValidation;

namespace Application.Tasks.Commands.ChangeTaskStatus;

internal sealed class ChangeTaskStatusCommandValidator : AbstractValidator<ChangeTaskStatusCommand>
{
    public ChangeTaskStatusCommandValidator()
    {
        RuleFor(x => x.TaskId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

